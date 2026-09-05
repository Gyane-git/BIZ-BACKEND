using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesQuotationService : ISalesQuotationService
{
    private readonly TenantDbContext _context;

    public SalesQuotationService(TenantDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<IEnumerable<SalesQuotationDto>> GetAllAsync()
    {
        var quotations = await _context.SalesQuotations
            .Include(x => x.SalesQuotationLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return quotations.Select(MapToDto);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<SalesQuotationDto?> GetByIdAsync(int id)
    {
        var quotation = await _context.SalesQuotations
            .Include(x => x.SalesQuotationLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (quotation == null)
            return null;

        return MapToDto(quotation);
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<SalesQuotationDto> CreateAsync(SalesQuotationDto dto)
    {
        // -----------------------------------------------------
        // Basic validation
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(dto.QuotationNumber))
            throw new ArgumentException("Quotation number is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException("At least one quotation line is required.");

        if (dto.ExchangeRate <= 0)
            throw new ArgumentException("Exchange rate must be greater than zero.");

        // -----------------------------------------------------
        // Duplicate quotation number
        // -----------------------------------------------------

        var quotationExists = await _context.SalesQuotations
            .AnyAsync(x => x.QuotationNumber == dto.QuotationNumber);

        if (quotationExists)
            throw new ArgumentException(
                $"Quotation number '{dto.QuotationNumber}' already exists.");

        // -----------------------------------------------------
        // Validate Fiscal Year
        // -----------------------------------------------------

        var fiscalYearExists = await _context.FiscalYears
            .AnyAsync(x => x.Id == dto.FiscalYearId);

        if (!fiscalYearExists)
            throw new ArgumentException("Fiscal year not found.");

        // -----------------------------------------------------
        // Validate Fiscal Year Period
        // -----------------------------------------------------

        var fiscalPeriod = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.FiscalYearId == dto.FiscalYearId);

        if (fiscalPeriod == null)
            throw new ArgumentException(
                "Fiscal year period not found or does not belong to the selected fiscal year.");

        // -----------------------------------------------------
        // Validate quotation date
        // -----------------------------------------------------

        if (dto.QuotationDate.Date < fiscalPeriod.StartDate.Date ||
            dto.QuotationDate.Date > fiscalPeriod.EndDate.Date)
        {
            throw new ArgumentException(
                "Quotation date must be within the selected fiscal year period.");
        }

        // -----------------------------------------------------
        // Calculate totals
        // -----------------------------------------------------

        decimal subTotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        var lines = new List<SalesQuotationLine>();

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            // ---------------------------------------------
            // Line validation
            // ---------------------------------------------

            if (lineDto.ProductId <= 0)
                throw new ArgumentException(
                    $"Invalid ProductId on line {lineDto.LineNumber}.");

            if (lineDto.Quantity <= 0)
                throw new ArgumentException(
                    $"Quantity must be greater than zero on line {lineDto.LineNumber}.");

            if (lineDto.UnitPrice < 0)
                throw new ArgumentException(
                    $"Unit price cannot be negative on line {lineDto.LineNumber}.");

            if (lineDto.DiscountPercent < 0 ||
                lineDto.DiscountPercent > 100)
            {
                throw new ArgumentException(
                    $"Discount percent must be between 0 and 100 on line {lineDto.LineNumber}.");
            }

            if (lineDto.TaxPercent < 0 ||
                lineDto.TaxPercent > 100)
            {
                throw new ArgumentException(
                    $"Tax percent must be between 0 and 100 on line {lineDto.LineNumber}.");
            }

            // ---------------------------------------------
            // Calculation
            // ---------------------------------------------

            decimal grossAmount =
                lineDto.Quantity * lineDto.UnitPrice;

            decimal discountAmount =
                grossAmount * lineDto.DiscountPercent / 100m;

            decimal taxableAmount =
                grossAmount - discountAmount;

            decimal taxAmount =
                taxableAmount * lineDto.TaxPercent / 100m;

            decimal lineTotal =
                taxableAmount + taxAmount;

            // ---------------------------------------------
            // Totals
            // ---------------------------------------------

            subTotal += grossAmount;
            totalDiscount += discountAmount;
            totalTax += taxAmount;

            // ---------------------------------------------
            // Entity
            // ---------------------------------------------

            lines.Add(new SalesQuotationLine
            {
                ProductId = lineDto.ProductId,
                UnitId = lineDto.UnitId,
                Description = lineDto.Description,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                DiscountPercent = lineDto.DiscountPercent,
                DiscountAmount = discountAmount,
                TaxPercent = lineDto.TaxPercent,
                TaxAmount = taxAmount,
                LineTotal = lineTotal,
                LineNumber = lineDto.LineNumber
            });
        }

        // -----------------------------------------------------
        // Grand Total
        // -----------------------------------------------------

        decimal grandTotal =
            subTotal - totalDiscount + totalTax;

        // -----------------------------------------------------
        // Create quotation
        // -----------------------------------------------------

        var quotation = new SalesQuotation
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            CustomerId = dto.CustomerId,

            QuotationNumber = dto.QuotationNumber,
            QuotationDate = dto.QuotationDate,
            ValidUntil = dto.ValidUntil,

            CurrencyId = dto.CurrencyId,
            ExchangeRate = dto.ExchangeRate,

            SubTotal = subTotal,
            DiscountAmount = totalDiscount,
            TaxAmount = totalTax,
            GrandTotal = grandTotal,

            Status = "Draft",

            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,

            BranchId = dto.BranchId,
            WarehouseId = dto.WarehouseId,

            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // -----------------------------------------------------
        // Add lines
        // -----------------------------------------------------

        quotation.SalesQuotationLines = lines;

        // -----------------------------------------------------
        // Save
        // -----------------------------------------------------

        _context.SalesQuotations.Add(quotation);

        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // Return created quotation
        // -----------------------------------------------------

        return MapToDto(quotation);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        SalesQuotationDto dto)
    {
        var quotation = await _context.SalesQuotations
            .Include(x => x.SalesQuotationLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (quotation == null)
            return false;

        // -----------------------------------------------------
        // Only Draft quotation can be edited
        // -----------------------------------------------------

        if (!string.Equals(
                quotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft sales quotations can be updated.");
        }

        // -----------------------------------------------------
        // Validation
        // -----------------------------------------------------

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one quotation line is required.");

        if (dto.ExchangeRate <= 0)
            throw new ArgumentException(
                "Exchange rate must be greater than zero.");

        // -----------------------------------------------------
        // Calculate totals again
        // -----------------------------------------------------

        decimal subTotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            if (lineDto.ProductId <= 0)
                throw new ArgumentException(
                    $"Invalid ProductId on line {lineDto.LineNumber}.");

            if (lineDto.Quantity <= 0)
                throw new ArgumentException(
                    $"Quantity must be greater than zero on line {lineDto.LineNumber}.");

            if (lineDto.UnitPrice < 0)
                throw new ArgumentException(
                    $"Unit price cannot be negative on line {lineDto.LineNumber}.");

            if (lineDto.DiscountPercent < 0 ||
                lineDto.DiscountPercent > 100)
            {
                throw new ArgumentException(
                    $"Discount percent must be between 0 and 100 on line {lineDto.LineNumber}.");
            }

            if (lineDto.TaxPercent < 0 ||
                lineDto.TaxPercent > 100)
            {
                throw new ArgumentException(
                    $"Tax percent must be between 0 and 100 on line {lineDto.LineNumber}.");
            }

            decimal grossAmount =
                lineDto.Quantity * lineDto.UnitPrice;

            decimal discountAmount =
                grossAmount * lineDto.DiscountPercent / 100m;

            decimal taxableAmount =
                grossAmount - discountAmount;

            decimal taxAmount =
                taxableAmount * lineDto.TaxPercent / 100m;

            subTotal += grossAmount;
            totalDiscount += discountAmount;
            totalTax += taxAmount;
        }

        decimal grandTotal =
            subTotal - totalDiscount + totalTax;

        // -----------------------------------------------------
        // Update header
        // -----------------------------------------------------

        quotation.QuotationDate = dto.QuotationDate;
        quotation.ValidUntil = dto.ValidUntil;

        quotation.CurrencyId = dto.CurrencyId;
        quotation.ExchangeRate = dto.ExchangeRate;

        quotation.ReferenceNumber = dto.ReferenceNumber;
        quotation.Notes = dto.Notes;

        quotation.BranchId = dto.BranchId;
        quotation.WarehouseId = dto.WarehouseId;

        quotation.Status = dto.Status;

        quotation.SubTotal = subTotal;
        quotation.DiscountAmount = totalDiscount;
        quotation.TaxAmount = totalTax;
        quotation.GrandTotal = grandTotal;

        quotation.UpdatedAt = DateTime.UtcNow;

        // -----------------------------------------------------
        // Remove old lines
        // -----------------------------------------------------

        _context.SalesQuotationLines.RemoveRange(
            quotation.SalesQuotationLines);

        // -----------------------------------------------------
        // Add new lines
        // -----------------------------------------------------

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            decimal grossAmount =
                lineDto.Quantity * lineDto.UnitPrice;

            decimal discountAmount =
                grossAmount * lineDto.DiscountPercent / 100m;

            decimal taxableAmount =
                grossAmount - discountAmount;

            decimal taxAmount =
                taxableAmount * lineDto.TaxPercent / 100m;

            decimal lineTotal =
                taxableAmount + taxAmount;

            quotation.SalesQuotationLines.Add(
                new SalesQuotationLine
                {
                    SalesQuotationId = quotation.Id,

                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,
                    Description = lineDto.Description,

                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,

                    DiscountPercent = lineDto.DiscountPercent,
                    DiscountAmount = discountAmount,

                    TaxPercent = lineDto.TaxPercent,
                    TaxAmount = taxAmount,

                    LineTotal = lineTotal,
                    LineNumber = lineDto.LineNumber
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // DELETE - SOFT DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var quotation = await _context.SalesQuotations
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (quotation == null)
            return false;

        quotation.IsActive = false;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // MAPPING
    // =========================================================

    private static SalesQuotationDto MapToDto(
        SalesQuotation quotation)
    {
        return new SalesQuotationDto
        {
            Id = quotation.Id,

            FiscalYearId = quotation.FiscalYearId,
            FiscalYearPeriodId = quotation.FiscalYearPeriodId,
            CustomerId = quotation.CustomerId,

            QuotationNumber = quotation.QuotationNumber,
            QuotationDate = quotation.QuotationDate,
            ValidUntil = quotation.ValidUntil,

            CurrencyId = quotation.CurrencyId,
            ExchangeRate = quotation.ExchangeRate,

            SubTotal = quotation.SubTotal,
            DiscountAmount = quotation.DiscountAmount,
            TaxAmount = quotation.TaxAmount,
            GrandTotal = quotation.GrandTotal,

            Status = quotation.Status,

            ReferenceNumber = quotation.ReferenceNumber,
            Notes = quotation.Notes,

            BranchId = quotation.BranchId,
            WarehouseId = quotation.WarehouseId,

            IsActive = quotation.IsActive,

            CreatedAt = quotation.CreatedAt,
            UpdatedAt = quotation.UpdatedAt,

            Lines = quotation.SalesQuotationLines
                .OrderBy(x => x.LineNumber)
                .Select(x => new SalesQuotationLineDto
                {
                    Id = x.Id,
                    SalesQuotationId = x.SalesQuotationId,

                    ProductId = x.ProductId,
                    UnitId = x.UnitId,

                    Description = x.Description,

                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,

                    DiscountPercent = x.DiscountPercent,
                    DiscountAmount = x.DiscountAmount,

                    TaxPercent = x.TaxPercent,
                    TaxAmount = x.TaxAmount,

                    LineTotal = x.LineTotal,

                    LineNumber = x.LineNumber
                })
                .ToList()
        };
    }
}
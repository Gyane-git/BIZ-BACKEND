using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseQuotationService : IPurchaseQuotationService
{
    private readonly TenantDbContext _context;

    public PurchaseQuotationService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseQuotationDto>> GetAllAsync()
    {
        return await _context.PurchaseQuotations
            .AsNoTracking()
            .Include(x => x.PurchaseQuotationLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new PurchaseQuotationDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                PurchaseRequestId = x.PurchaseRequestId,
                QuotationNumber = x.QuotationNumber,
                QuotationDate = x.QuotationDate,
                ValidUntil = x.ValidUntil,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                GrandTotal = x.GrandTotal,
                Status = x.Status,
                ReferenceNumber = x.ReferenceNumber,
                Notes = x.Notes,
                BranchId = x.BranchId,
                WarehouseId = x.WarehouseId,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Lines = x.PurchaseQuotationLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new PurchaseQuotationLineDto
                    {
                        Id = l.Id,
                        PurchaseQuotationId = l.PurchaseQuotationId,
                        ProductId = l.ProductId,
                        UnitId = l.UnitId,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountPercent = l.DiscountPercent,
                        DiscountAmount = l.DiscountAmount,
                        TaxPercent = l.TaxPercent,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal,
                        LineNumber = l.LineNumber
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<PurchaseQuotationDto?> GetByIdAsync(int id)
    {
        var quotation = await _context.PurchaseQuotations
            .AsNoTracking()
            .Include(x => x.PurchaseQuotationLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (quotation == null)
            return null;

        return MapToDto(quotation);
    }

    public async Task<PurchaseQuotationDto> CreateAsync(
        PurchaseQuotationDto dto)
    {
        var quotationNumber = dto.QuotationNumber.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(quotationNumber))
            throw new ArgumentException("Quotation number is required.");

        if (dto.SupplierId <= 0)
            throw new ArgumentException("Valid SupplierId is required.");

        if (dto.FiscalYearId <= 0)
            throw new ArgumentException("Valid FiscalYearId is required.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new ArgumentException("Valid FiscalYearPeriodId is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one quotation line is required.");

        if (dto.ExchangeRate <= 0)
            throw new ArgumentException(
                "ExchangeRate must be greater than zero.");

        var duplicate = await _context.PurchaseQuotations
            .AnyAsync(x => x.QuotationNumber == quotationNumber);

        if (duplicate)
            throw new ArgumentException(
                $"Quotation number '{quotationNumber}' already exists.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new ArgumentException("Invalid FiscalYear.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new ArgumentException("Invalid FiscalYearPeriod.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new ArgumentException(
                "FiscalYearPeriod does not belong to the selected FiscalYear.");

        if (dto.QuotationDate < period.StartDate ||
            dto.QuotationDate > period.EndDate)
        {
            throw new ArgumentException(
                "QuotationDate must be within the selected fiscal period.");
        }

        var validStatuses = new[] { "Draft" };

        var status = string.IsNullOrWhiteSpace(dto.Status)
            ? "Draft"
            : dto.Status.Trim();

        if (!validStatuses.Contains(status,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "New PurchaseQuotation must have Draft status.");
        }

        var validProducts = await _context.Products
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        var lineNumbers = new HashSet<int>();

        foreach (var line in dto.Lines)
        {
            if (line.ProductId <= 0)
                throw new ArgumentException(
                    "Every line must have a valid ProductId.");

            if (!validProducts.Contains(line.ProductId))
                throw new ArgumentException(
                    $"ProductId {line.ProductId} is invalid or inactive.");

            if (line.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");

            if (line.UnitPrice < 0)
                throw new ArgumentException(
                    "UnitPrice cannot be negative.");

            if (line.DiscountPercent < 0 ||
                line.DiscountPercent > 100)
                throw new ArgumentException(
                    "DiscountPercent must be between 0 and 100.");

            if (line.TaxPercent < 0 ||
                line.TaxPercent > 100)
                throw new ArgumentException(
                    "TaxPercent must be between 0 and 100.");

            if (line.LineNumber <= 0)
                throw new ArgumentException(
                    "LineNumber must be greater than zero.");

            if (!lineNumbers.Add(line.LineNumber))
                throw new ArgumentException(
                    $"Duplicate LineNumber {line.LineNumber}.");
        }

        var quotation = new PurchaseQuotation
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            SupplierId = dto.SupplierId,
            PurchaseRequestId = dto.PurchaseRequestId,
            QuotationNumber = quotationNumber,
            QuotationDate = dto.QuotationDate,
            ValidUntil = dto.ValidUntil,
            CurrencyId = dto.CurrencyId,
            ExchangeRate = dto.ExchangeRate,
            Status = "Draft",
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            BranchId = dto.BranchId,
            WarehouseId = dto.WarehouseId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        decimal subtotal = 0;
        decimal discountTotal = 0;
        decimal taxTotal = 0;

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            var gross = lineDto.Quantity * lineDto.UnitPrice;

            var discount =
                gross * lineDto.DiscountPercent / 100m;

            var taxableAmount = gross - discount;

            var tax =
                taxableAmount * lineDto.TaxPercent / 100m;

            var lineTotal =
                taxableAmount + tax;

            subtotal += gross;
            discountTotal += discount;
            taxTotal += tax;

            quotation.PurchaseQuotationLines.Add(
                new PurchaseQuotationLine
                {
                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,
                    Description = lineDto.Description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    DiscountPercent = lineDto.DiscountPercent,
                    DiscountAmount = discount,
                    TaxPercent = lineDto.TaxPercent,
                    TaxAmount = tax,
                    LineTotal = lineTotal,
                    LineNumber = lineDto.LineNumber
                });
        }

        quotation.SubTotal = subtotal;
        quotation.DiscountAmount = discountTotal;
        quotation.TaxAmount = taxTotal;
        quotation.GrandTotal =
            subtotal - discountTotal + taxTotal;

        _context.PurchaseQuotations.Add(quotation);

        await _context.SaveChangesAsync();

        return MapToDto(quotation);
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseQuotationDto dto)
    {
        var quotation = await _context.PurchaseQuotations
            .Include(x => x.PurchaseQuotationLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (quotation == null)
            return false;

        if (!string.Equals(
                quotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft PurchaseQuotation can be updated.");
        }

        if (dto.SupplierId <= 0)
            throw new ArgumentException("Valid SupplierId is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one quotation line is required.");

        var quotationNumber =
            dto.QuotationNumber.Trim().ToUpperInvariant();

        var duplicate = await _context.PurchaseQuotations
            .AnyAsync(x =>
                x.Id != id &&
                x.QuotationNumber == quotationNumber);

        if (duplicate)
            throw new ArgumentException(
                $"Quotation number '{quotationNumber}' already exists.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new ArgumentException(
                "Invalid FiscalYearPeriod.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new ArgumentException(
                "FiscalYearPeriod does not belong to FiscalYear.");

        if (dto.QuotationDate < period.StartDate ||
            dto.QuotationDate > period.EndDate)
        {
            throw new ArgumentException(
                "QuotationDate must be within fiscal period.");
        }

        var validProducts = await _context.Products
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        var lineNumbers = new HashSet<int>();

        foreach (var line in dto.Lines)
        {
            if (line.ProductId <= 0 ||
                !validProducts.Contains(line.ProductId))
                throw new ArgumentException(
                    $"ProductId {line.ProductId} is invalid or inactive.");

            if (line.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");

            if (line.UnitPrice < 0)
                throw new ArgumentException(
                    "UnitPrice cannot be negative.");

            if (line.DiscountPercent < 0 ||
                line.DiscountPercent > 100)
                throw new ArgumentException(
                    "DiscountPercent must be between 0 and 100.");

            if (line.TaxPercent < 0 ||
                line.TaxPercent > 100)
                throw new ArgumentException(
                    "TaxPercent must be between 0 and 100.");

            if (line.LineNumber <= 0)
                throw new ArgumentException(
                    "LineNumber must be greater than zero.");

            if (!lineNumbers.Add(line.LineNumber))
                throw new ArgumentException(
                    $"Duplicate LineNumber {line.LineNumber}.");
        }

        quotation.FiscalYearId = dto.FiscalYearId;
        quotation.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        quotation.SupplierId = dto.SupplierId;
        quotation.PurchaseRequestId = dto.PurchaseRequestId;
        quotation.QuotationNumber = quotationNumber;
        quotation.QuotationDate = dto.QuotationDate;
        quotation.ValidUntil = dto.ValidUntil;
        quotation.CurrencyId = dto.CurrencyId;
        quotation.ExchangeRate = dto.ExchangeRate;
        quotation.ReferenceNumber = dto.ReferenceNumber;
        quotation.Notes = dto.Notes;
        quotation.BranchId = dto.BranchId;
        quotation.WarehouseId = dto.WarehouseId;
        quotation.UpdatedAt = DateTime.UtcNow;

        _context.PurchaseQuotationLines.RemoveRange(
            quotation.PurchaseQuotationLines);

        quotation.PurchaseQuotationLines.Clear();

        decimal subtotal = 0;
        decimal discountTotal = 0;
        decimal taxTotal = 0;

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            var gross = lineDto.Quantity * lineDto.UnitPrice;

            var discount =
                gross * lineDto.DiscountPercent / 100m;

            var taxableAmount = gross - discount;

            var tax =
                taxableAmount * lineDto.TaxPercent / 100m;

            var lineTotal =
                taxableAmount + tax;

            subtotal += gross;
            discountTotal += discount;
            taxTotal += tax;

            quotation.PurchaseQuotationLines.Add(
                new PurchaseQuotationLine
                {
                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,
                    Description = lineDto.Description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    DiscountPercent = lineDto.DiscountPercent,
                    DiscountAmount = discount,
                    TaxPercent = lineDto.TaxPercent,
                    TaxAmount = tax,
                    LineTotal = lineTotal,
                    LineNumber = lineDto.LineNumber
                });
        }

        quotation.SubTotal = subtotal;
        quotation.DiscountAmount = discountTotal;
        quotation.TaxAmount = taxTotal;
        quotation.GrandTotal =
            subtotal - discountTotal + taxTotal;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quotation = await _context.PurchaseQuotations
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (quotation == null)
            return false;

        if (!string.Equals(
                quotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft PurchaseQuotation can be deleted.");
        }

        quotation.IsActive = false;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static PurchaseQuotationDto MapToDto(
        PurchaseQuotation x)
    {
        return new PurchaseQuotationDto
        {
            Id = x.Id,
            FiscalYearId = x.FiscalYearId,
            FiscalYearPeriodId = x.FiscalYearPeriodId,
            SupplierId = x.SupplierId,
            PurchaseRequestId = x.PurchaseRequestId,
            QuotationNumber = x.QuotationNumber,
            QuotationDate = x.QuotationDate,
            ValidUntil = x.ValidUntil,
            CurrencyId = x.CurrencyId,
            ExchangeRate = x.ExchangeRate,
            SubTotal = x.SubTotal,
            DiscountAmount = x.DiscountAmount,
            TaxAmount = x.TaxAmount,
            GrandTotal = x.GrandTotal,
            Status = x.Status,
            ReferenceNumber = x.ReferenceNumber,
            Notes = x.Notes,
            BranchId = x.BranchId,
            WarehouseId = x.WarehouseId,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,

            Lines = x.PurchaseQuotationLines
                .OrderBy(l => l.LineNumber)
                .Select(l => new PurchaseQuotationLineDto
                {
                    Id = l.Id,
                    PurchaseQuotationId = l.PurchaseQuotationId,
                    ProductId = l.ProductId,
                    UnitId = l.UnitId,
                    Description = l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountPercent = l.DiscountPercent,
                    DiscountAmount = l.DiscountAmount,
                    TaxPercent = l.TaxPercent,
                    TaxAmount = l.TaxAmount,
                    LineTotal = l.LineTotal,
                    LineNumber = l.LineNumber
                })
                .ToList()
        };
    }
}
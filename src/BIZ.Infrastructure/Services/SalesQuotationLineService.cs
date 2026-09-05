using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesQuotationLineService : ISalesQuotationLineService
{
    private readonly TenantDbContext _context;

    public SalesQuotationLineService(TenantDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<IEnumerable<SalesQuotationLineDto>> GetAllAsync()
    {
        var lines = await _context.SalesQuotationLines
            .Where(x => x.SalesQuotation.IsActive)
            .OrderBy(x => x.SalesQuotationId)
            .ThenBy(x => x.LineNumber)
            .ToListAsync();

        return lines.Select(MapToDto);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<SalesQuotationLineDto?> GetByIdAsync(int id)
    {
        var line = await _context.SalesQuotationLines
            .Include(x => x.SalesQuotation)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.SalesQuotation.IsActive);

        if (line == null)
            return null;

        return MapToDto(line);
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<SalesQuotationLineDto> CreateAsync(
        SalesQuotationLineDto dto)
    {
        // -----------------------------------------------------
        // Validate quotation
        // -----------------------------------------------------

        if (dto.SalesQuotationId <= 0)
            throw new ArgumentException(
                "Sales quotation ID is required.");

        var quotation = await _context.SalesQuotations
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SalesQuotationId &&
                x.IsActive);

        if (quotation == null)
            throw new ArgumentException(
                "Sales quotation not found.");

        // -----------------------------------------------------
        // Only Draft quotation can be modified
        // -----------------------------------------------------

        if (!string.Equals(
                quotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be added to a Draft sales quotation.");
        }

        // -----------------------------------------------------
        // Validate Product
        // -----------------------------------------------------

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "Product ID is required.");

        // -----------------------------------------------------
        // Validate Quantity
        // -----------------------------------------------------

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        // -----------------------------------------------------
        // Validate Unit Price
        // -----------------------------------------------------

        if (dto.UnitPrice < 0)
            throw new ArgumentException(
                "Unit price cannot be negative.");

        // -----------------------------------------------------
        // Validate Discount
        // -----------------------------------------------------

        if (dto.DiscountPercent < 0 ||
            dto.DiscountPercent > 100)
        {
            throw new ArgumentException(
                "Discount percent must be between 0 and 100.");
        }

        // -----------------------------------------------------
        // Validate Tax
        // -----------------------------------------------------

        if (dto.TaxPercent < 0 ||
            dto.TaxPercent > 100)
        {
            throw new ArgumentException(
                "Tax percent must be between 0 and 100.");
        }

        // -----------------------------------------------------
        // Check duplicate line number
        // -----------------------------------------------------

        var lineNumberExists =
            await _context.SalesQuotationLines.AnyAsync(x =>
                x.SalesQuotationId == dto.SalesQuotationId &&
                x.LineNumber == dto.LineNumber);

        if (lineNumberExists)
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this quotation.");

        // -----------------------------------------------------
        // Calculate
        // -----------------------------------------------------

        var calculation = CalculateLine(dto);

        // -----------------------------------------------------
        // Create Entity
        // -----------------------------------------------------

        var line = new SalesQuotationLine
        {
            SalesQuotationId = dto.SalesQuotationId,

            ProductId = dto.ProductId,
            UnitId = dto.UnitId,

            Description = dto.Description,

            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,

            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = calculation.DiscountAmount,

            TaxPercent = dto.TaxPercent,
            TaxAmount = calculation.TaxAmount,

            LineTotal = calculation.LineTotal,

            LineNumber = dto.LineNumber
        };

        _context.SalesQuotationLines.Add(line);

        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // Recalculate quotation totals
        // -----------------------------------------------------

        await RecalculateQuotationAsync(dto.SalesQuotationId);

        return MapToDto(line);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        SalesQuotationLineDto dto)
    {
        var line = await _context.SalesQuotationLines
            .Include(x => x.SalesQuotation)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        // -----------------------------------------------------
        // Quotation must be Draft
        // -----------------------------------------------------

        if (!string.Equals(
                line.SalesQuotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be updated when the sales quotation is Draft.");
        }

        // -----------------------------------------------------
        // Validate
        // -----------------------------------------------------

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "Product ID is required.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitPrice < 0)
            throw new ArgumentException(
                "Unit price cannot be negative.");

        if (dto.DiscountPercent < 0 ||
            dto.DiscountPercent > 100)
        {
            throw new ArgumentException(
                "Discount percent must be between 0 and 100.");
        }

        if (dto.TaxPercent < 0 ||
            dto.TaxPercent > 100)
        {
            throw new ArgumentException(
                "Tax percent must be between 0 and 100.");
        }

        // -----------------------------------------------------
        // Keep original quotation
        // -----------------------------------------------------

        var quotationId = line.SalesQuotationId;

        // -----------------------------------------------------
        // Check duplicate line number
        // -----------------------------------------------------

        var duplicateLine =
            await _context.SalesQuotationLines.AnyAsync(x =>
                x.Id != id &&
                x.SalesQuotationId == quotationId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this quotation.");

        // -----------------------------------------------------
        // Calculate
        // -----------------------------------------------------

        var calculation = CalculateLine(dto);

        // -----------------------------------------------------
        // Update
        // -----------------------------------------------------

        line.ProductId = dto.ProductId;
        line.UnitId = dto.UnitId;
        line.Description = dto.Description;

        line.Quantity = dto.Quantity;
        line.UnitPrice = dto.UnitPrice;

        line.DiscountPercent = dto.DiscountPercent;
        line.DiscountAmount = calculation.DiscountAmount;

        line.TaxPercent = dto.TaxPercent;
        line.TaxAmount = calculation.TaxAmount;

        line.LineTotal = calculation.LineTotal;

        line.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // Recalculate quotation
        // -----------------------------------------------------

        await RecalculateQuotationAsync(quotationId);

        return true;
    }

    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.SalesQuotationLines
            .Include(x => x.SalesQuotation)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        // -----------------------------------------------------
        // Quotation must be Draft
        // -----------------------------------------------------

        if (!string.Equals(
                line.SalesQuotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be deleted when the sales quotation is Draft.");
        }

        var quotationId = line.SalesQuotationId;

        _context.SalesQuotationLines.Remove(line);

        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // Recalculate quotation
        // -----------------------------------------------------

        await RecalculateQuotationAsync(quotationId);

        return true;
    }

    // =========================================================
    // CALCULATE LINE
    // =========================================================

    private static (
        decimal GrossAmount,
        decimal DiscountAmount,
        decimal TaxableAmount,
        decimal TaxAmount,
        decimal LineTotal
    ) CalculateLine(SalesQuotationLineDto dto)
    {
        decimal grossAmount =
            dto.Quantity * dto.UnitPrice;

        decimal discountAmount =
            grossAmount * dto.DiscountPercent / 100m;

        decimal taxableAmount =
            grossAmount - discountAmount;

        decimal taxAmount =
            taxableAmount * dto.TaxPercent / 100m;

        decimal lineTotal =
            taxableAmount + taxAmount;

        return (
            grossAmount,
            discountAmount,
            taxableAmount,
            taxAmount,
            lineTotal
        );
    }

    // =========================================================
    // RECALCULATE QUOTATION TOTALS
    // =========================================================

    private async Task RecalculateQuotationAsync(
        int quotationId)
    {
        var quotation = await _context.SalesQuotations
            .Include(x => x.SalesQuotationLines)
            .FirstOrDefaultAsync(x => x.Id == quotationId);

        if (quotation == null)
            return;

        decimal subTotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var line in quotation.SalesQuotationLines)
        {
            subTotal +=
                line.Quantity * line.UnitPrice;

            totalDiscount +=
                line.DiscountAmount;

            totalTax +=
                line.TaxAmount;
        }

        quotation.SubTotal = subTotal;
        quotation.DiscountAmount = totalDiscount;
        quotation.TaxAmount = totalTax;

        quotation.GrandTotal =
            subTotal - totalDiscount + totalTax;

        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // =========================================================
    // MAP ENTITY → DTO
    // =========================================================

    private static SalesQuotationLineDto MapToDto(
        SalesQuotationLine line)
    {
        return new SalesQuotationLineDto
        {
            Id = line.Id,
            SalesQuotationId = line.SalesQuotationId,

            ProductId = line.ProductId,
            UnitId = line.UnitId,

            Description = line.Description,

            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,

            DiscountPercent = line.DiscountPercent,
            DiscountAmount = line.DiscountAmount,

            TaxPercent = line.TaxPercent,
            TaxAmount = line.TaxAmount,

            LineTotal = line.LineTotal,

            LineNumber = line.LineNumber
        };
    }
}
using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseQuotationLineService
    : IPurchaseQuotationLineService
{
    private readonly TenantDbContext _context;

    public PurchaseQuotationLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseQuotationLineDto>> GetAllAsync()
    {
        return await _context.PurchaseQuotationLines
            .AsNoTracking()
            .Where(x => x.PurchaseQuotation.IsActive)
            .OrderBy(x => x.PurchaseQuotationId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new PurchaseQuotationLineDto
            {
                Id = x.Id,
                PurchaseQuotationId = x.PurchaseQuotationId,
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
            .ToListAsync();
    }

    public async Task<PurchaseQuotationLineDto?> GetByIdAsync(int id)
    {
        return await _context.PurchaseQuotationLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.PurchaseQuotation.IsActive)
            .Select(x => new PurchaseQuotationLineDto
            {
                Id = x.Id,
                PurchaseQuotationId = x.PurchaseQuotationId,
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
            .FirstOrDefaultAsync();
    }

    public async Task<PurchaseQuotationLineDto> CreateAsync(
        PurchaseQuotationLineDto dto)
    {
        var quotation = await _context.PurchaseQuotations
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseQuotationId &&
                x.IsActive);

        if (quotation == null)
            throw new ArgumentException(
                "PurchaseQuotation not found.");

        if (!string.Equals(
                quotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be added to Draft quotation.");
        }

        if (dto.ProductId <= 0)
            throw new ArgumentException("Valid ProductId is required.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitPrice < 0)
            throw new ArgumentException(
                "UnitPrice cannot be negative.");

        if (dto.DiscountPercent < 0 ||
            dto.DiscountPercent > 100)
            throw new ArgumentException(
                "DiscountPercent must be between 0 and 100.");

        if (dto.TaxPercent < 0 ||
            dto.TaxPercent > 100)
            throw new ArgumentException(
                "TaxPercent must be between 0 and 100.");

        if (dto.LineNumber <= 0)
            throw new ArgumentException(
                "LineNumber must be greater than zero.");

        var productExists = await _context.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
            throw new ArgumentException(
                "Product not found or inactive.");

        var duplicateLine = await _context.PurchaseQuotationLines
            .AnyAsync(x =>
                x.PurchaseQuotationId == dto.PurchaseQuotationId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new ArgumentException(
                $"LineNumber {dto.LineNumber} already exists.");

        var gross = dto.Quantity * dto.UnitPrice;

        var discount =
            gross * dto.DiscountPercent / 100m;

        var taxable =
            gross - discount;

        var tax =
            taxable * dto.TaxPercent / 100m;

        var lineTotal =
            taxable + tax;

        var line = new PurchaseQuotationLine
        {
            PurchaseQuotationId = dto.PurchaseQuotationId,
            ProductId = dto.ProductId,
            UnitId = dto.UnitId,
            Description = dto.Description,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = discount,
            TaxPercent = dto.TaxPercent,
            TaxAmount = tax,
            LineTotal = lineTotal,
            LineNumber = dto.LineNumber
        };

        _context.PurchaseQuotationLines.Add(line);

        await _context.SaveChangesAsync();

        return new PurchaseQuotationLineDto
        {
            Id = line.Id,
            PurchaseQuotationId = line.PurchaseQuotationId,
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

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseQuotationLineDto dto)
    {
        var line = await _context.PurchaseQuotationLines
            .Include(x => x.PurchaseQuotation)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.PurchaseQuotation.IsActive);

        if (line == null)
            return false;

        if (!string.Equals(
                line.PurchaseQuotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft quotation lines can be updated.");
        }

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitPrice < 0)
            throw new ArgumentException(
                "UnitPrice cannot be negative.");

        if (dto.LineNumber <= 0)
            throw new ArgumentException(
                "LineNumber must be greater than zero.");

        var duplicate = await _context.PurchaseQuotationLines
            .AnyAsync(x =>
                x.Id != id &&
                x.PurchaseQuotationId ==
                    line.PurchaseQuotationId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                $"LineNumber {dto.LineNumber} already exists.");

        var gross = dto.Quantity * dto.UnitPrice;

        var discount =
            gross * dto.DiscountPercent / 100m;

        var taxable =
            gross - discount;

        var tax =
            taxable * dto.TaxPercent / 100m;

        var lineTotal =
            taxable + tax;

        line.ProductId = dto.ProductId;
        line.UnitId = dto.UnitId;
        line.Description = dto.Description;
        line.Quantity = dto.Quantity;
        line.UnitPrice = dto.UnitPrice;
        line.DiscountPercent = dto.DiscountPercent;
        line.DiscountAmount = discount;
        line.TaxPercent = dto.TaxPercent;
        line.TaxAmount = tax;
        line.LineTotal = lineTotal;
        line.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.PurchaseQuotationLines
            .Include(x => x.PurchaseQuotation)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.PurchaseQuotation.IsActive);

        if (line == null)
            return false;

        if (!string.Equals(
                line.PurchaseQuotation.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft quotation lines can be deleted.");
        }

        _context.PurchaseQuotationLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }
}
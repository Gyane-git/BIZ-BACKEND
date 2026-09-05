using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseReturnLineService
    : IPurchaseReturnLineService
{
    private readonly TenantDbContext _context;

    public PurchaseReturnLineService(
        TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseReturnLineDto>> GetAllAsync()
    {
        return await _context.PurchaseReturnLines
            .AsNoTracking()
            .Where(x => x.PurchaseReturn.IsActive)
            .OrderBy(x => x.PurchaseReturnId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new PurchaseReturnLineDto
            {
                Id = x.Id,
                PurchaseReturnId =
                    x.PurchaseReturnId,
                PurchaseInvoiceLineId =
                    x.PurchaseInvoiceLineId,
                GoodsReceiptLineId =
                    x.GoodsReceiptLineId,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DiscountPercent =
                    x.DiscountPercent,
                DiscountAmount =
                    x.DiscountAmount,
                TaxPercent = x.TaxPercent,
                TaxAmount = x.TaxAmount,
                LineTotal = x.LineTotal,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<PurchaseReturnLineDto?> GetByIdAsync(
        int id)
    {
        return await _context.PurchaseReturnLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.PurchaseReturn.IsActive)
            .Select(x => new PurchaseReturnLineDto
            {
                Id = x.Id,
                PurchaseReturnId =
                    x.PurchaseReturnId,
                PurchaseInvoiceLineId =
                    x.PurchaseInvoiceLineId,
                GoodsReceiptLineId =
                    x.GoodsReceiptLineId,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DiscountPercent =
                    x.DiscountPercent,
                DiscountAmount =
                    x.DiscountAmount,
                TaxPercent = x.TaxPercent,
                TaxAmount = x.TaxAmount,
                LineTotal = x.LineTotal,
                LineNumber = x.LineNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PurchaseReturnLineDto> CreateAsync(
        PurchaseReturnLineDto dto)
    {
        var parent =
            await _context.PurchaseReturns
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.PurchaseReturnId &&
                    x.IsActive);

        if (parent == null)
            throw new InvalidOperationException(
                "Purchase Return not found or inactive.");

        if (parent.IsPosted ||
            parent.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft Purchase Return can have lines added.");
        }

        Validate(dto);

        var duplicate =
            await _context.PurchaseReturnLines
                .AnyAsync(x =>
                    x.PurchaseReturnId ==
                        dto.PurchaseReturnId &&
                    x.LineNumber ==
                        dto.LineNumber);

        if (duplicate)
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var calculation =
            Calculate(dto);

        var line = new PurchaseReturnLine
        {
            PurchaseReturnId =
                dto.PurchaseReturnId,

            PurchaseInvoiceLineId =
                dto.PurchaseInvoiceLineId,

            GoodsReceiptLineId =
                dto.GoodsReceiptLineId,

            ProductId = dto.ProductId,
            UnitId = dto.UnitId,
            Description = dto.Description,

            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,

            DiscountPercent =
                dto.DiscountPercent,

            DiscountAmount =
                calculation.Discount,

            TaxPercent =
                dto.TaxPercent,

            TaxAmount =
                calculation.Tax,

            LineTotal =
                calculation.LineTotal,

            LineNumber =
                dto.LineNumber
        };

        _context.PurchaseReturnLines.Add(line);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseReturnLineDto dto)
    {
        var line =
            await _context.PurchaseReturnLines
                .Include(x => x.PurchaseReturn)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!line.PurchaseReturn.IsActive)
            throw new InvalidOperationException(
                "Purchase Return is inactive.");

        if (line.PurchaseReturn.IsPosted ||
            line.PurchaseReturn.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft Purchase Return lines can be updated.");
        }

        Validate(dto);

        var duplicate =
            await _context.PurchaseReturnLines
                .AnyAsync(x =>
                    x.Id != id &&
                    x.PurchaseReturnId ==
                        line.PurchaseReturnId &&
                    x.LineNumber ==
                        dto.LineNumber);

        if (duplicate)
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var calculation =
            Calculate(dto);

        line.PurchaseInvoiceLineId =
            dto.PurchaseInvoiceLineId;

        line.GoodsReceiptLineId =
            dto.GoodsReceiptLineId;

        line.ProductId = dto.ProductId;
        line.UnitId = dto.UnitId;
        line.Description = dto.Description;

        line.Quantity = dto.Quantity;
        line.UnitPrice = dto.UnitPrice;

        line.DiscountPercent =
            dto.DiscountPercent;

        line.DiscountAmount =
            calculation.Discount;

        line.TaxPercent =
            dto.TaxPercent;

        line.TaxAmount =
            calculation.Tax;

        line.LineTotal =
            calculation.LineTotal;

        line.LineNumber =
            dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line =
            await _context.PurchaseReturnLines
                .Include(x => x.PurchaseReturn)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (line.PurchaseReturn.IsPosted ||
            line.PurchaseReturn.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft Purchase Return lines can be deleted.");
        }

        _context.PurchaseReturnLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }

    private static void Validate(
        PurchaseReturnLineDto dto)
    {
        if (dto.ProductId <= 0)
            throw new InvalidOperationException(
                "ProductId is required.");

        if (dto.Quantity <= 0)
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");

        if (dto.UnitPrice < 0)
            throw new InvalidOperationException(
                "UnitPrice cannot be negative.");

        if (dto.DiscountPercent < 0 ||
            dto.DiscountPercent > 100)
        {
            throw new InvalidOperationException(
                "DiscountPercent must be between 0 and 100.");
        }

        if (dto.TaxPercent < 0 ||
            dto.TaxPercent > 100)
        {
            throw new InvalidOperationException(
                "TaxPercent must be between 0 and 100.");
        }
    }

    private static (
        decimal Discount,
        decimal Tax,
        decimal LineTotal)
        Calculate(PurchaseReturnLineDto dto)
    {
        var gross =
            dto.Quantity * dto.UnitPrice;

        var discount =
            gross *
            dto.DiscountPercent / 100m;

        var taxable =
            gross - discount;

        var tax =
            taxable *
            dto.TaxPercent / 100m;

        return (
            discount,
            tax,
            taxable + tax);
    }
}
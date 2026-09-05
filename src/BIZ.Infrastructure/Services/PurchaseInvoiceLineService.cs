using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseInvoiceLineService
    : IPurchaseInvoiceLineService
{
    private readonly TenantDbContext _context;

    public PurchaseInvoiceLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseInvoiceLineDto>> GetAllAsync()
    {
        return await _context.PurchaseInvoiceLines
            .AsNoTracking()
            .Where(x => x.PurchaseInvoice.IsActive)
            .OrderBy(x => x.PurchaseInvoiceId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new PurchaseInvoiceLineDto
            {
                Id = x.Id,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                GoodsReceiptLineId = x.GoodsReceiptLineId,
                PurchaseOrderLineId = x.PurchaseOrderLineId,
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

    public async Task<PurchaseInvoiceLineDto?> GetByIdAsync(int id)
    {
        return await _context.PurchaseInvoiceLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.PurchaseInvoice.IsActive)
            .Select(x => new PurchaseInvoiceLineDto
            {
                Id = x.Id,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                GoodsReceiptLineId = x.GoodsReceiptLineId,
                PurchaseOrderLineId = x.PurchaseOrderLineId,
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

    public async Task<PurchaseInvoiceLineDto> CreateAsync(
        PurchaseInvoiceLineDto dto)
    {
        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseInvoiceId &&
                x.IsActive);

        if (invoice == null)
            throw new InvalidOperationException(
                "Purchase Invoice not found or inactive.");

        if (invoice.IsPosted ||
            invoice.Status != "Draft")
            throw new InvalidOperationException(
                "Only Draft Purchase Invoice can have lines added.");

        if (dto.Quantity <= 0)
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");

        if (dto.UnitPrice < 0)
            throw new InvalidOperationException(
                "UnitPrice cannot be negative.");

        if (dto.DiscountPercent < 0 ||
            dto.DiscountPercent > 100)
            throw new InvalidOperationException(
                "DiscountPercent must be between 0 and 100.");

        if (dto.TaxPercent < 0 ||
            dto.TaxPercent > 100)
            throw new InvalidOperationException(
                "TaxPercent must be between 0 and 100.");

        var duplicate = await _context.PurchaseInvoiceLines
            .AnyAsync(x =>
                x.PurchaseInvoiceId == dto.PurchaseInvoiceId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var gross =
            dto.Quantity * dto.UnitPrice;

        var discount =
            gross * dto.DiscountPercent / 100m;

        var taxable =
            gross - discount;

        var tax =
            taxable * dto.TaxPercent / 100m;

        var lineTotal =
            taxable + tax;

        var line = new PurchaseInvoiceLine
        {
            PurchaseInvoiceId = dto.PurchaseInvoiceId,
            GoodsReceiptLineId = dto.GoodsReceiptLineId,
            PurchaseOrderLineId = dto.PurchaseOrderLineId,
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

        _context.PurchaseInvoiceLines.Add(line);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseInvoiceLineDto dto)
    {
        var line = await _context.PurchaseInvoiceLines
            .Include(x => x.PurchaseInvoice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!line.PurchaseInvoice.IsActive)
            throw new InvalidOperationException(
                "Purchase Invoice is inactive.");

        if (line.PurchaseInvoice.IsPosted ||
            line.PurchaseInvoice.Status != "Draft")
            throw new InvalidOperationException(
                "Only Draft Purchase Invoice lines can be updated.");

        if (dto.Quantity <= 0)
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");

        if (dto.UnitPrice < 0)
            throw new InvalidOperationException(
                "UnitPrice cannot be negative.");

        if (dto.DiscountPercent < 0 ||
            dto.DiscountPercent > 100)
            throw new InvalidOperationException(
                "DiscountPercent must be between 0 and 100.");

        if (dto.TaxPercent < 0 ||
            dto.TaxPercent > 100)
            throw new InvalidOperationException(
                "TaxPercent must be between 0 and 100.");

        var duplicate = await _context.PurchaseInvoiceLines
            .AnyAsync(x =>
                x.Id != id &&
                x.PurchaseInvoiceId ==
                    line.PurchaseInvoiceId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var gross =
            dto.Quantity * dto.UnitPrice;

        var discount =
            gross * dto.DiscountPercent / 100m;

        var taxable =
            gross - discount;

        var tax =
            taxable * dto.TaxPercent / 100m;

        line.GoodsReceiptLineId =
            dto.GoodsReceiptLineId;

        line.PurchaseOrderLineId =
            dto.PurchaseOrderLineId;

        line.ProductId = dto.ProductId;
        line.UnitId = dto.UnitId;
        line.Description = dto.Description;
        line.Quantity = dto.Quantity;
        line.UnitPrice = dto.UnitPrice;

        line.DiscountPercent =
            dto.DiscountPercent;

        line.DiscountAmount =
            discount;

        line.TaxPercent =
            dto.TaxPercent;

        line.TaxAmount =
            tax;

        line.LineTotal =
            taxable + tax;

        line.LineNumber =
            dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.PurchaseInvoiceLines
            .Include(x => x.PurchaseInvoice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (line.PurchaseInvoice.IsPosted ||
            line.PurchaseInvoice.Status != "Draft")
            throw new InvalidOperationException(
                "Only Draft Purchase Invoice lines can be deleted.");

        _context.PurchaseInvoiceLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }
}
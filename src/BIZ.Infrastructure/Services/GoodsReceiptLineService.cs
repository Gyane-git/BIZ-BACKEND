using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class GoodsReceiptLineService : IGoodsReceiptLineService
{
    private readonly TenantDbContext _context;

    public GoodsReceiptLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GoodsReceiptLineDto>> GetAllAsync()
    {
        return await _context.GoodsReceiptLines
            .AsNoTracking()
            .Where(x => x.GoodsReceipt.IsActive)
            .OrderBy(x => x.GoodsReceiptId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new GoodsReceiptLineDto
            {
                Id = x.Id,
                GoodsReceiptId = x.GoodsReceiptId,
                PurchaseOrderLineId = x.PurchaseOrderLineId,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                Description = x.Description,
                OrderedQuantity = x.OrderedQuantity,
                ReceivedQuantity = x.ReceivedQuantity,
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

    public async Task<GoodsReceiptLineDto?> GetByIdAsync(int id)
    {
        return await _context.GoodsReceiptLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.GoodsReceipt.IsActive)
            .Select(x => new GoodsReceiptLineDto
            {
                Id = x.Id,
                GoodsReceiptId = x.GoodsReceiptId,
                PurchaseOrderLineId = x.PurchaseOrderLineId,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                Description = x.Description,
                OrderedQuantity = x.OrderedQuantity,
                ReceivedQuantity = x.ReceivedQuantity,
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

    public async Task<GoodsReceiptLineDto> CreateAsync(
        GoodsReceiptLineDto dto)
    {
        var receipt = await _context.GoodsReceipts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.GoodsReceiptId &&
                x.IsActive);

        if (receipt == null)
            throw new InvalidOperationException(
                "Goods Receipt not found or inactive.");

        if (receipt.Status != "Draft")
            throw new InvalidOperationException(
                "Only Draft Goods Receipt can have lines added.");

        if (dto.ReceivedQuantity <= 0)
            throw new InvalidOperationException(
                "ReceivedQuantity must be greater than zero.");

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

        var poLine = await _context.PurchaseOrderLines
            .Include(x => x.PurchaseOrder)
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseOrderLineId &&
                x.PurchaseOrderId == receipt.PurchaseOrderId);

        if (poLine == null)
            throw new InvalidOperationException(
                "PurchaseOrderLine does not belong to the Goods Receipt Purchase Order.");

        if (poLine.ProductId != dto.ProductId)
            throw new InvalidOperationException(
                "Product does not match PurchaseOrderLine.");

        var existingLineNumber = await _context.GoodsReceiptLines
            .AnyAsync(x =>
                x.GoodsReceiptId == dto.GoodsReceiptId &&
                x.LineNumber == dto.LineNumber);

        if (existingLineNumber)
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var receivedAlready = await _context.GoodsReceiptLines
            .Where(x =>
                x.PurchaseOrderLineId == poLine.Id &&
                x.GoodsReceipt.IsActive)
            .SumAsync(x => (decimal?)x.ReceivedQuantity) ?? 0;

        var remaining =
            poLine.Quantity - receivedAlready;

        if (dto.ReceivedQuantity > remaining)
            throw new InvalidOperationException(
                $"Received quantity exceeds remaining quantity. Remaining: {remaining}.");

        var gross =
            dto.ReceivedQuantity * dto.UnitPrice;

        var discount =
            gross * dto.DiscountPercent / 100m;

        var taxable =
            gross - discount;

        var tax =
            taxable * dto.TaxPercent / 100m;

        var lineTotal =
            taxable + tax;

        var line = new GoodsReceiptLine
        {
            GoodsReceiptId = dto.GoodsReceiptId,
            PurchaseOrderLineId = dto.PurchaseOrderLineId,
            ProductId = dto.ProductId,
            UnitId = dto.UnitId,
            Description = dto.Description,
            OrderedQuantity = poLine.Quantity,
            ReceivedQuantity = dto.ReceivedQuantity,
            UnitPrice = dto.UnitPrice,
            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = discount,
            TaxPercent = dto.TaxPercent,
            TaxAmount = tax,
            LineTotal = lineTotal,
            LineNumber = dto.LineNumber
        };

        _context.GoodsReceiptLines.Add(line);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(line.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        GoodsReceiptLineDto dto)
    {
        var line = await _context.GoodsReceiptLines
            .Include(x => x.GoodsReceipt)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!line.GoodsReceipt.IsActive)
            throw new InvalidOperationException(
                "Goods Receipt is inactive.");

        if (line.GoodsReceipt.Status != "Draft")
            throw new InvalidOperationException(
                "Only Draft Goods Receipt lines can be updated.");

        if (dto.ReceivedQuantity <= 0)
            throw new InvalidOperationException(
                "ReceivedQuantity must be greater than zero.");

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

        var duplicate = await _context.GoodsReceiptLines
            .AnyAsync(x =>
                x.Id != id &&
                x.GoodsReceiptId == line.GoodsReceiptId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var poLine = await _context.PurchaseOrderLines
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseOrderLineId &&
                x.PurchaseOrderId == line.GoodsReceipt.PurchaseOrderId);

        if (poLine == null)
            throw new InvalidOperationException(
                "Invalid PurchaseOrderLine.");

        if (poLine.ProductId != dto.ProductId)
            throw new InvalidOperationException(
                "Product does not match PurchaseOrderLine.");

        var receivedAlready = await _context.GoodsReceiptLines
            .Where(x =>
                x.Id != id &&
                x.PurchaseOrderLineId == poLine.Id &&
                x.GoodsReceipt.IsActive)
            .SumAsync(x => (decimal?)x.ReceivedQuantity) ?? 0;

        var remaining =
            poLine.Quantity - receivedAlready;

        if (dto.ReceivedQuantity > remaining)
            throw new InvalidOperationException(
                $"Received quantity exceeds remaining quantity. Remaining: {remaining}.");

        var gross =
            dto.ReceivedQuantity * dto.UnitPrice;

        var discount =
            gross * dto.DiscountPercent / 100m;

        var taxable =
            gross - discount;

        var tax =
            taxable * dto.TaxPercent / 100m;

        line.PurchaseOrderLineId = dto.PurchaseOrderLineId;
        line.ProductId = dto.ProductId;
        line.UnitId = dto.UnitId;
        line.Description = dto.Description;
        line.OrderedQuantity = poLine.Quantity;
        line.ReceivedQuantity = dto.ReceivedQuantity;
        line.UnitPrice = dto.UnitPrice;
        line.DiscountPercent = dto.DiscountPercent;
        line.DiscountAmount = discount;
        line.TaxPercent = dto.TaxPercent;
        line.TaxAmount = tax;
        line.LineTotal = taxable + tax;
        line.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.GoodsReceiptLines
            .Include(x => x.GoodsReceipt)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (line.GoodsReceipt.Status != "Draft")
            throw new InvalidOperationException(
                "Only Draft Goods Receipt lines can be deleted.");

        _context.GoodsReceiptLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }
}
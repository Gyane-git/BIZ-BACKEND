using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly TenantDbContext _context;

    public GoodsReceiptService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GoodsReceiptDto>> GetAllAsync()
    {
        return await _context.GoodsReceipts
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.GoodsReceiptLines)
            .OrderByDescending(x => x.Id)
            .Select(x => new GoodsReceiptDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                PurchaseOrderId = x.PurchaseOrderId,
                ReceiptNumber = x.ReceiptNumber,
                ReceiptDate = x.ReceiptDate,
                WarehouseId = x.WarehouseId,
                ReferenceNumber = x.ReferenceNumber,
                Notes = x.Notes,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                GrandTotal = x.GrandTotal,
                Status = x.Status,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Lines = x.GoodsReceiptLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new GoodsReceiptLineDto
                    {
                        Id = l.Id,
                        GoodsReceiptId = l.GoodsReceiptId,
                        PurchaseOrderLineId = l.PurchaseOrderLineId,
                        ProductId = l.ProductId,
                        UnitId = l.UnitId,
                        Description = l.Description,
                        OrderedQuantity = l.OrderedQuantity,
                        ReceivedQuantity = l.ReceivedQuantity,
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

    public async Task<GoodsReceiptDto?> GetByIdAsync(int id)
    {
        return await _context.GoodsReceipts
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Include(x => x.GoodsReceiptLines)
            .Select(x => new GoodsReceiptDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                PurchaseOrderId = x.PurchaseOrderId,
                ReceiptNumber = x.ReceiptNumber,
                ReceiptDate = x.ReceiptDate,
                WarehouseId = x.WarehouseId,
                ReferenceNumber = x.ReferenceNumber,
                Notes = x.Notes,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                GrandTotal = x.GrandTotal,
                Status = x.Status,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Lines = x.GoodsReceiptLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new GoodsReceiptLineDto
                    {
                        Id = l.Id,
                        GoodsReceiptId = l.GoodsReceiptId,
                        PurchaseOrderLineId = l.PurchaseOrderLineId,
                        ProductId = l.ProductId,
                        UnitId = l.UnitId,
                        Description = l.Description,
                        OrderedQuantity = l.OrderedQuantity,
                        ReceivedQuantity = l.ReceivedQuantity,
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
            .FirstOrDefaultAsync();
    }

    public async Task<GoodsReceiptDto> CreateAsync(GoodsReceiptDto dto)
    {
        var receiptNumber = dto.ReceiptNumber.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(receiptNumber))
            throw new InvalidOperationException("Receipt number is required.");

        if (dto.PurchaseOrderId <= 0)
            throw new InvalidOperationException("PurchaseOrderId is required.");

        if (dto.SupplierId <= 0)
            throw new InvalidOperationException("SupplierId is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one goods receipt line is required.");

        var duplicate = await _context.GoodsReceipts
            .AnyAsync(x =>
                x.ReceiptNumber == receiptNumber &&
                x.IsActive);

        if (duplicate)
            throw new InvalidOperationException(
                $"Receipt number '{receiptNumber}' already exists.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new InvalidOperationException("Invalid or inactive FiscalYear.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new InvalidOperationException(
                "Invalid or inactive FiscalYearPeriod.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new InvalidOperationException(
                "FiscalYearPeriod does not belong to the selected FiscalYear.");

        if (dto.ReceiptDate.Date < period.StartDate.Date ||
            dto.ReceiptDate.Date > period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "ReceiptDate must be within the selected fiscal period.");
        }

        var purchaseOrder = await _context.PurchaseOrders
            .Include(x => x.PurchaseOrderLines)
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseOrderId &&
                x.IsActive);

        if (purchaseOrder == null)
            throw new InvalidOperationException(
                "Purchase Order not found or inactive.");

        if (purchaseOrder.Status != "Draft" &&
            purchaseOrder.Status != "Approved" &&
            purchaseOrder.Status != "Confirmed")
        {
            throw new InvalidOperationException(
                "Goods Receipt cannot be created for the current Purchase Order status.");
        }

        if (purchaseOrder.SupplierId != dto.SupplierId)
            throw new InvalidOperationException(
                "Supplier does not match the Purchase Order.");

        var lineNumbers = dto.Lines.Select(x => x.LineNumber).ToList();

        if (lineNumbers.Any(x => x <= 0))
            throw new InvalidOperationException(
                "LineNumber must be greater than zero.");

        if (lineNumbers.Distinct().Count() != lineNumbers.Count)
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var receipt = new GoodsReceipt
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            SupplierId = dto.SupplierId,
            PurchaseOrderId = dto.PurchaseOrderId,
            ReceiptNumber = receiptNumber,
            ReceiptDate = dto.ReceiptDate,
            WarehouseId = dto.WarehouseId,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            Status = "Draft",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;
        decimal grandTotal = 0;

        foreach (var lineDto in dto.Lines)
        {
            if (lineDto.PurchaseOrderLineId <= 0)
                throw new InvalidOperationException(
                    "PurchaseOrderLineId is required.");

            if (lineDto.ReceivedQuantity <= 0)
                throw new InvalidOperationException(
                    "ReceivedQuantity must be greater than zero.");

            if (lineDto.UnitPrice < 0)
                throw new InvalidOperationException(
                    "UnitPrice cannot be negative.");

            if (lineDto.DiscountPercent < 0 ||
                lineDto.DiscountPercent > 100)
            {
                throw new InvalidOperationException(
                    "DiscountPercent must be between 0 and 100.");
            }

            if (lineDto.TaxPercent < 0 ||
                lineDto.TaxPercent > 100)
            {
                throw new InvalidOperationException(
                    "TaxPercent must be between 0 and 100.");
            }

            var poLine = purchaseOrder.PurchaseOrderLines
                .FirstOrDefault(x =>
                    x.Id == lineDto.PurchaseOrderLineId);

            if (poLine == null)
                throw new InvalidOperationException(
                    $"PurchaseOrderLine {lineDto.PurchaseOrderLineId} does not belong to the selected Purchase Order.");

            if (poLine.ProductId != lineDto.ProductId)
                throw new InvalidOperationException(
                    $"Product does not match PurchaseOrderLine {poLine.Id}.");

            var alreadyReceived = await _context.GoodsReceiptLines
                .Where(x =>
                    x.PurchaseOrderLineId == poLine.Id &&
                    x.GoodsReceipt.IsActive)
                .SumAsync(x => (decimal?)x.ReceivedQuantity) ?? 0;

            var remainingQuantity =
                poLine.Quantity - alreadyReceived;

            if (lineDto.ReceivedQuantity > remainingQuantity)
            {
                throw new InvalidOperationException(
                    $"Received quantity exceeds remaining quantity for PurchaseOrderLine {poLine.Id}. Remaining: {remainingQuantity}.");
            }

            var gross =
                lineDto.ReceivedQuantity *
                lineDto.UnitPrice;

            var discount =
                gross *
                lineDto.DiscountPercent / 100m;

            var taxable =
                gross - discount;

            var tax =
                taxable *
                lineDto.TaxPercent / 100m;

            var lineTotal =
                taxable + tax;

            var line = new GoodsReceiptLine
            {
                PurchaseOrderLineId = poLine.Id,
                ProductId = lineDto.ProductId,
                UnitId = lineDto.UnitId,
                Description = lineDto.Description,
                OrderedQuantity = poLine.Quantity,
                ReceivedQuantity = lineDto.ReceivedQuantity,
                UnitPrice = lineDto.UnitPrice,
                DiscountPercent = lineDto.DiscountPercent,
                DiscountAmount = discount,
                TaxPercent = lineDto.TaxPercent,
                TaxAmount = tax,
                LineTotal = lineTotal,
                LineNumber = lineDto.LineNumber
            };

            receipt.GoodsReceiptLines.Add(line);

            subtotal += gross;
            totalDiscount += discount;
            totalTax += tax;
            grandTotal += lineTotal;
        }

        receipt.SubTotal = subtotal;
        receipt.DiscountAmount = totalDiscount;
        receipt.TaxAmount = totalTax;
        receipt.GrandTotal = grandTotal;

        _context.GoodsReceipts.Add(receipt);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(receipt.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, GoodsReceiptDto dto)
    {
        var receipt = await _context.GoodsReceipts
            .Include(x => x.GoodsReceiptLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (receipt == null)
            return false;

        if (receipt.Status != "Draft")
            throw new InvalidOperationException(
                "Only Draft Goods Receipt can be updated.");

        dto.ReceiptNumber = dto.ReceiptNumber.Trim()
            .ToUpperInvariant();

        var duplicate = await _context.GoodsReceipts
            .AnyAsync(x =>
                x.Id != id &&
                x.ReceiptNumber == dto.ReceiptNumber &&
                x.IsActive);

        if (duplicate)
            throw new InvalidOperationException(
                $"Receipt number '{dto.ReceiptNumber}' already exists.");

        var purchaseOrder = await _context.PurchaseOrders
            .Include(x => x.PurchaseOrderLines)
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseOrderId &&
                x.IsActive);

        if (purchaseOrder == null)
            throw new InvalidOperationException(
                "Purchase Order not found or inactive.");

        if (purchaseOrder.SupplierId != dto.SupplierId)
            throw new InvalidOperationException(
                "Supplier does not match Purchase Order.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one goods receipt line is required.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new InvalidOperationException(
                "Invalid or inactive FiscalYear.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null ||
            period.FiscalYearId != dto.FiscalYearId)
        {
            throw new InvalidOperationException(
                "Invalid FiscalYearPeriod.");
        }

        if (dto.ReceiptDate.Date < period.StartDate.Date ||
            dto.ReceiptDate.Date > period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "ReceiptDate must be within the selected fiscal period.");
        }

        _context.GoodsReceiptLines.RemoveRange(
            receipt.GoodsReceiptLines);

        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;
        decimal grandTotal = 0;

        var lineNumbers = dto.Lines
            .Select(x => x.LineNumber)
            .ToList();

        if (lineNumbers.Any(x => x <= 0) ||
            lineNumbers.Distinct().Count() != lineNumbers.Count)
        {
            throw new InvalidOperationException(
                "LineNumber must be unique and greater than zero.");
        }

        foreach (var lineDto in dto.Lines)
        {
            if (lineDto.ReceivedQuantity <= 0)
                throw new InvalidOperationException(
                    "ReceivedQuantity must be greater than zero.");

            if (lineDto.UnitPrice < 0)
                throw new InvalidOperationException(
                    "UnitPrice cannot be negative.");

            if (lineDto.DiscountPercent < 0 ||
                lineDto.DiscountPercent > 100)
                throw new InvalidOperationException(
                    "DiscountPercent must be between 0 and 100.");

            if (lineDto.TaxPercent < 0 ||
                lineDto.TaxPercent > 100)
                throw new InvalidOperationException(
                    "TaxPercent must be between 0 and 100.");

            var poLine = purchaseOrder.PurchaseOrderLines
                .FirstOrDefault(x =>
                    x.Id == lineDto.PurchaseOrderLineId);

            if (poLine == null)
                throw new InvalidOperationException(
                    $"PurchaseOrderLine {lineDto.PurchaseOrderLineId} not found.");

            if (poLine.ProductId != lineDto.ProductId)
                throw new InvalidOperationException(
                    $"Product does not match PurchaseOrderLine {poLine.Id}.");

            var otherReceived = await _context.GoodsReceiptLines
                .Where(x =>
                    x.PurchaseOrderLineId == poLine.Id &&
                    x.GoodsReceiptId != id &&
                    x.GoodsReceipt.IsActive)
                .SumAsync(x => (decimal?)x.ReceivedQuantity) ?? 0;

            var remainingQuantity =
                poLine.Quantity - otherReceived;

            if (lineDto.ReceivedQuantity > remainingQuantity)
                throw new InvalidOperationException(
                    $"Received quantity exceeds remaining quantity for PurchaseOrderLine {poLine.Id}. Remaining: {remainingQuantity}.");

            var gross =
                lineDto.ReceivedQuantity *
                lineDto.UnitPrice;

            var discount =
                gross *
                lineDto.DiscountPercent / 100m;

            var taxable =
                gross - discount;

            var tax =
                taxable *
                lineDto.TaxPercent / 100m;

            var lineTotal =
                taxable + tax;

            receipt.GoodsReceiptLines.Add(
                new GoodsReceiptLine
                {
                    GoodsReceiptId = id,
                    PurchaseOrderLineId = poLine.Id,
                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,
                    Description = lineDto.Description,
                    OrderedQuantity = poLine.Quantity,
                    ReceivedQuantity = lineDto.ReceivedQuantity,
                    UnitPrice = lineDto.UnitPrice,
                    DiscountPercent = lineDto.DiscountPercent,
                    DiscountAmount = discount,
                    TaxPercent = lineDto.TaxPercent,
                    TaxAmount = tax,
                    LineTotal = lineTotal,
                    LineNumber = lineDto.LineNumber
                });

            subtotal += gross;
            totalDiscount += discount;
            totalTax += tax;
            grandTotal += lineTotal;
        }

        receipt.FiscalYearId = dto.FiscalYearId;
        receipt.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        receipt.SupplierId = dto.SupplierId;
        receipt.PurchaseOrderId = dto.PurchaseOrderId;
        receipt.ReceiptNumber = dto.ReceiptNumber;
        receipt.ReceiptDate = dto.ReceiptDate;
        receipt.WarehouseId = dto.WarehouseId;
        receipt.ReferenceNumber = dto.ReferenceNumber;
        receipt.Notes = dto.Notes;

        receipt.SubTotal = subtotal;
        receipt.DiscountAmount = totalDiscount;
        receipt.TaxAmount = totalTax;
        receipt.GrandTotal = grandTotal;

        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var receipt = await _context.GoodsReceipts
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (receipt == null)
            return false;

        if (receipt.Status != "Draft")
            throw new InvalidOperationException(
                "Only Draft Goods Receipt can be deleted.");

        receipt.IsActive = false;
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
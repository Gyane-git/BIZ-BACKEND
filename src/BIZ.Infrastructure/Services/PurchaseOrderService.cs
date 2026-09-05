using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly TenantDbContext _context;

    public PurchaseOrderService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> GetAllAsync()
    {
        return await _context.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.PurchaseOrderLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new PurchaseOrderDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                PurchaseRequestId = x.PurchaseRequestId,
                PurchaseQuotationId = x.PurchaseQuotationId,
                OrderNumber = x.OrderNumber,
                OrderDate = x.OrderDate,
                ExpectedDeliveryDate = x.ExpectedDeliveryDate,
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

                Lines = x.PurchaseOrderLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new PurchaseOrderLineDto
                    {
                        Id = l.Id,
                        PurchaseOrderId = l.PurchaseOrderId,
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

    public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
    {
        var order = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(x => x.PurchaseOrderLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        return order == null ? null : MapToDto(order);
    }

    public async Task<PurchaseOrderDto> CreateAsync(
        PurchaseOrderDto dto)
    {
        var orderNumber =
            dto.OrderNumber.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException(
                "Order number is required.");

        if (dto.SupplierId <= 0)
            throw new ArgumentException(
                "Valid SupplierId is required.");

        if (dto.FiscalYearId <= 0)
            throw new ArgumentException(
                "Valid FiscalYearId is required.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new ArgumentException(
                "Valid FiscalYearPeriodId is required.");

        if (dto.ExchangeRate <= 0)
            throw new ArgumentException(
                "ExchangeRate must be greater than zero.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one purchase order line is required.");

        var duplicate = await _context.PurchaseOrders
            .AnyAsync(x => x.OrderNumber == orderNumber);

        if (duplicate)
            throw new ArgumentException(
                $"Order number '{orderNumber}' already exists.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new ArgumentException(
                "Invalid FiscalYear.");

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

        if (dto.OrderDate < period.StartDate ||
            dto.OrderDate > period.EndDate)
        {
            throw new ArgumentException(
                "OrderDate must be within the selected fiscal period.");
        }

        var status = string.IsNullOrWhiteSpace(dto.Status)
            ? "Draft"
            : dto.Status.Trim();

        if (!status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "New PurchaseOrder must have Draft status.");
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
            {
                throw new ArgumentException(
                    $"ProductId {line.ProductId} is invalid or inactive.");
            }

            if (line.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");

            if (line.UnitPrice < 0)
                throw new ArgumentException(
                    "UnitPrice cannot be negative.");

            if (line.DiscountPercent < 0 ||
                line.DiscountPercent > 100)
            {
                throw new ArgumentException(
                    "DiscountPercent must be between 0 and 100.");
            }

            if (line.TaxPercent < 0 ||
                line.TaxPercent > 100)
            {
                throw new ArgumentException(
                    "TaxPercent must be between 0 and 100.");
            }

            if (line.LineNumber <= 0)
                throw new ArgumentException(
                    "LineNumber must be greater than zero.");

            if (!lineNumbers.Add(line.LineNumber))
                throw new ArgumentException(
                    $"Duplicate LineNumber {line.LineNumber}.");
        }

        var order = new PurchaseOrder
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            SupplierId = dto.SupplierId,
            PurchaseRequestId = dto.PurchaseRequestId,
            PurchaseQuotationId = dto.PurchaseQuotationId,
            OrderNumber = orderNumber,
            OrderDate = dto.OrderDate,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
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

        decimal subtotal = 0m;
        decimal discountTotal = 0m;
        decimal taxTotal = 0m;

        foreach (var lineDto in
                 dto.Lines.OrderBy(x => x.LineNumber))
        {
            var gross =
                lineDto.Quantity * lineDto.UnitPrice;

            var discount =
                gross * lineDto.DiscountPercent / 100m;

            var taxable =
                gross - discount;

            var tax =
                taxable * lineDto.TaxPercent / 100m;

            var lineTotal =
                taxable + tax;

            subtotal += gross;
            discountTotal += discount;
            taxTotal += tax;

            order.PurchaseOrderLines.Add(
                new PurchaseOrderLine
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

        order.SubTotal = subtotal;
        order.DiscountAmount = discountTotal;
        order.TaxAmount = taxTotal;
        order.GrandTotal =
            subtotal - discountTotal + taxTotal;

        _context.PurchaseOrders.Add(order);

        await _context.SaveChangesAsync();

        return MapToDto(order);
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseOrderDto dto)
    {
        var order = await _context.PurchaseOrders
            .Include(x => x.PurchaseOrderLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (order == null)
            return false;

        if (!order.Status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft PurchaseOrder can be updated.");
        }

        if (dto.SupplierId <= 0)
            throw new ArgumentException(
                "Valid SupplierId is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one purchase order line is required.");

        var orderNumber =
            dto.OrderNumber.Trim().ToUpperInvariant();

        var duplicate = await _context.PurchaseOrders
            .AnyAsync(x =>
                x.Id != id &&
                x.OrderNumber == orderNumber);

        if (duplicate)
            throw new ArgumentException(
                $"Order number '{orderNumber}' already exists.");

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

        if (dto.OrderDate < period.StartDate ||
            dto.OrderDate > period.EndDate)
        {
            throw new ArgumentException(
                "OrderDate must be within fiscal period.");
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
            {
                throw new ArgumentException(
                    $"ProductId {line.ProductId} is invalid or inactive.");
            }

            if (line.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");

            if (line.UnitPrice < 0)
                throw new ArgumentException(
                    "UnitPrice cannot be negative.");

            if (line.DiscountPercent < 0 ||
                line.DiscountPercent > 100)
            {
                throw new ArgumentException(
                    "DiscountPercent must be between 0 and 100.");
            }

            if (line.TaxPercent < 0 ||
                line.TaxPercent > 100)
            {
                throw new ArgumentException(
                    "TaxPercent must be between 0 and 100.");
            }

            if (line.LineNumber <= 0)
                throw new ArgumentException(
                    "LineNumber must be greater than zero.");

            if (!lineNumbers.Add(line.LineNumber))
                throw new ArgumentException(
                    $"Duplicate LineNumber {line.LineNumber}.");
        }

        order.FiscalYearId = dto.FiscalYearId;
        order.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        order.SupplierId = dto.SupplierId;
        order.PurchaseRequestId = dto.PurchaseRequestId;
        order.PurchaseQuotationId = dto.PurchaseQuotationId;
        order.OrderNumber = orderNumber;
        order.OrderDate = dto.OrderDate;
        order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        order.CurrencyId = dto.CurrencyId;
        order.ExchangeRate = dto.ExchangeRate;
        order.ReferenceNumber = dto.ReferenceNumber;
        order.Notes = dto.Notes;
        order.BranchId = dto.BranchId;
        order.WarehouseId = dto.WarehouseId;
        order.UpdatedAt = DateTime.UtcNow;

        _context.PurchaseOrderLines.RemoveRange(
            order.PurchaseOrderLines);

        order.PurchaseOrderLines.Clear();

        decimal subtotal = 0m;
        decimal discountTotal = 0m;
        decimal taxTotal = 0m;

        foreach (var lineDto in
                 dto.Lines.OrderBy(x => x.LineNumber))
        {
            var gross =
                lineDto.Quantity * lineDto.UnitPrice;

            var discount =
                gross * lineDto.DiscountPercent / 100m;

            var taxable =
                gross - discount;

            var tax =
                taxable * lineDto.TaxPercent / 100m;

            var lineTotal =
                taxable + tax;

            subtotal += gross;
            discountTotal += discount;
            taxTotal += tax;

            order.PurchaseOrderLines.Add(
                new PurchaseOrderLine
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

        order.SubTotal = subtotal;
        order.DiscountAmount = discountTotal;
        order.TaxAmount = taxTotal;
        order.GrandTotal =
            subtotal - discountTotal + taxTotal;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (order == null)
            return false;

        if (!order.Status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft PurchaseOrder can be deleted.");
        }

        order.IsActive = false;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static PurchaseOrderDto MapToDto(
        PurchaseOrder x)
    {
        return new PurchaseOrderDto
        {
            Id = x.Id,
            FiscalYearId = x.FiscalYearId,
            FiscalYearPeriodId = x.FiscalYearPeriodId,
            SupplierId = x.SupplierId,
            PurchaseRequestId = x.PurchaseRequestId,
            PurchaseQuotationId = x.PurchaseQuotationId,
            OrderNumber = x.OrderNumber,
            OrderDate = x.OrderDate,
            ExpectedDeliveryDate = x.ExpectedDeliveryDate,
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

            Lines = x.PurchaseOrderLines
                .OrderBy(l => l.LineNumber)
                .Select(l => new PurchaseOrderLineDto
                {
                    Id = l.Id,
                    PurchaseOrderId = l.PurchaseOrderId,
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
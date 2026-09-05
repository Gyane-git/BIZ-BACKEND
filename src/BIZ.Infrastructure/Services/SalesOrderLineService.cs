using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesOrderLineService : ISalesOrderLineService
{
    private readonly TenantDbContext _context;

    public SalesOrderLineService(TenantDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<IEnumerable<SalesOrderLineDto>> GetAllAsync()
    {
        var lines = await _context.SalesOrderLines
            .Include(x => x.SalesOrder)
            .Where(x => x.SalesOrder.IsActive)
            .OrderBy(x => x.SalesOrderId)
            .ThenBy(x => x.LineNumber)
            .ToListAsync();

        return lines.Select(MapToDto);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<SalesOrderLineDto?> GetByIdAsync(int id)
    {
        var line = await _context.SalesOrderLines
            .Include(x => x.SalesOrder)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.SalesOrder.IsActive);

        if (line == null)
            return null;

        return MapToDto(line);
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<SalesOrderLineDto> CreateAsync(
        SalesOrderLineDto dto)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SalesOrderId &&
                x.IsActive);

        if (order == null)
            throw new ArgumentException(
                "Sales order not found.");

        if (!string.Equals(
                order.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be added to a Draft sales order.");
        }

        ValidateLine(dto);

        var duplicate = await _context.SalesOrderLines
            .AnyAsync(x =>
                x.SalesOrderId == dto.SalesOrderId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this order.");

        var calculation = CalculateLine(dto);

        var line = new SalesOrderLine
        {
            SalesOrderId = dto.SalesOrderId,

            ProductId = dto.ProductId,
            UnitId = dto.UnitId,

            Description = dto.Description,

            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,

            DiscountPercent =
                dto.DiscountPercent,

            DiscountAmount =
                calculation.DiscountAmount,

            TaxPercent =
                dto.TaxPercent,

            TaxAmount =
                calculation.TaxAmount,

            LineTotal =
                calculation.LineTotal,

            LineNumber =
                dto.LineNumber
        };

        _context.SalesOrderLines.Add(line);

        await _context.SaveChangesAsync();

        await RecalculateOrderAsync(
            dto.SalesOrderId);

        return MapToDto(line);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        SalesOrderLineDto dto)
    {
        var line = await _context.SalesOrderLines
            .Include(x => x.SalesOrder)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!string.Equals(
                line.SalesOrder.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be updated when the sales order is Draft.");
        }

        ValidateLine(dto);

        var duplicate = await _context.SalesOrderLines
            .AnyAsync(x =>
                x.Id != id &&
                x.SalesOrderId == line.SalesOrderId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this order.");

        var orderId = line.SalesOrderId;

        var calculation = CalculateLine(dto);

        line.ProductId =
            dto.ProductId;

        line.UnitId =
            dto.UnitId;

        line.Description =
            dto.Description;

        line.Quantity =
            dto.Quantity;

        line.UnitPrice =
            dto.UnitPrice;

        line.DiscountPercent =
            dto.DiscountPercent;

        line.DiscountAmount =
            calculation.DiscountAmount;

        line.TaxPercent =
            dto.TaxPercent;

        line.TaxAmount =
            calculation.TaxAmount;

        line.LineTotal =
            calculation.LineTotal;

        line.LineNumber =
            dto.LineNumber;

        await _context.SaveChangesAsync();

        await RecalculateOrderAsync(orderId);

        return true;
    }

    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.SalesOrderLines
            .Include(x => x.SalesOrder)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!string.Equals(
                line.SalesOrder.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be deleted when the sales order is Draft.");
        }

        var orderId =
            line.SalesOrderId;

        _context.SalesOrderLines.Remove(line);

        await _context.SaveChangesAsync();

        await RecalculateOrderAsync(orderId);

        return true;
    }

    // =========================================================
    // VALIDATION
    // =========================================================

    private static void ValidateLine(
        SalesOrderLineDto line)
    {
        if (line.SalesOrderId <= 0)
            throw new ArgumentException(
                "Sales order ID is required.");

        if (line.ProductId <= 0)
            throw new ArgumentException(
                "Product ID is required.");

        if (line.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (line.UnitPrice < 0)
            throw new ArgumentException(
                "Unit price cannot be negative.");

        if (line.DiscountPercent < 0 ||
            line.DiscountPercent > 100)
        {
            throw new ArgumentException(
                "Discount percent must be between 0 and 100.");
        }

        if (line.TaxPercent < 0 ||
            line.TaxPercent > 100)
        {
            throw new ArgumentException(
                "Tax percent must be between 0 and 100.");
        }
    }

    // =========================================================
    // CALCULATE
    // =========================================================

    private static (
        decimal GrossAmount,
        decimal DiscountAmount,
        decimal TaxableAmount,
        decimal TaxAmount,
        decimal LineTotal
    ) CalculateLine(
        SalesOrderLineDto line)
    {
        decimal grossAmount =
            line.Quantity *
            line.UnitPrice;

        decimal discountAmount =
            grossAmount *
            line.DiscountPercent / 100m;

        decimal taxableAmount =
            grossAmount -
            discountAmount;

        decimal taxAmount =
            taxableAmount *
            line.TaxPercent / 100m;

        decimal lineTotal =
            taxableAmount +
            taxAmount;

        return (
            grossAmount,
            discountAmount,
            taxableAmount,
            taxAmount,
            lineTotal
        );
    }

    // =========================================================
    // RECALCULATE ORDER
    // =========================================================

    private async Task RecalculateOrderAsync(
        int orderId)
    {
        var order = await _context.SalesOrders
            .Include(x => x.SalesOrderLines)
            .FirstOrDefaultAsync(x =>
                x.Id == orderId);

        if (order == null)
            return;

        decimal subTotal = 0;
        decimal discount = 0;
        decimal tax = 0;

        foreach (var line in order.SalesOrderLines)
        {
            subTotal +=
                line.Quantity *
                line.UnitPrice;

            discount +=
                line.DiscountAmount;

            tax +=
                line.TaxAmount;
        }

        order.SubTotal =
            subTotal;

        order.DiscountAmount =
            discount;

        order.TaxAmount =
            tax;

        order.GrandTotal =
            subTotal -
            discount +
            tax;

        order.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // =========================================================
    // MAP
    // =========================================================

    private static SalesOrderLineDto MapToDto(
        SalesOrderLine line)
    {
        return new SalesOrderLineDto
        {
            Id = line.Id,

            SalesOrderId =
                line.SalesOrderId,

            ProductId =
                line.ProductId,

            UnitId =
                line.UnitId,

            Description =
                line.Description,

            Quantity =
                line.Quantity,

            UnitPrice =
                line.UnitPrice,

            DiscountPercent =
                line.DiscountPercent,

            DiscountAmount =
                line.DiscountAmount,

            TaxPercent =
                line.TaxPercent,

            TaxAmount =
                line.TaxAmount,

            LineTotal =
                line.LineTotal,

            LineNumber =
                line.LineNumber
        };
    }
}
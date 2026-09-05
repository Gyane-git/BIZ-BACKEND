using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesOrderService : ISalesOrderService
{
    private readonly TenantDbContext _context;

    public SalesOrderService(TenantDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<IEnumerable<SalesOrderDto>> GetAllAsync()
    {
        var orders = await _context.SalesOrders
            .Include(x => x.SalesOrderLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<SalesOrderDto?> GetByIdAsync(int id)
    {
        var order = await _context.SalesOrders
            .Include(x => x.SalesOrderLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (order == null)
            return null;

        return MapToDto(order);
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<SalesOrderDto> CreateAsync(
        SalesOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.OrderNumber))
            throw new ArgumentException(
                "Order number is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one sales order line is required.");

        if (dto.ExchangeRate <= 0)
            throw new ArgumentException(
                "Exchange rate must be greater than zero.");

        // -----------------------------------------------------
        // Duplicate order number
        // -----------------------------------------------------

        var exists = await _context.SalesOrders
            .AnyAsync(x =>
                x.OrderNumber == dto.OrderNumber);

        if (exists)
            throw new ArgumentException(
                $"Order number '{dto.OrderNumber}' already exists.");

        // -----------------------------------------------------
        // Fiscal Year
        // -----------------------------------------------------

        var fiscalYearExists =
            await _context.FiscalYears
                .AnyAsync(x => x.Id == dto.FiscalYearId);

        if (!fiscalYearExists)
            throw new ArgumentException(
                "Fiscal year not found.");

        // -----------------------------------------------------
        // Fiscal Period
        // -----------------------------------------------------

        var fiscalPeriod =
            await _context.FiscalYearPeriods
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.FiscalYearPeriodId &&
                    x.FiscalYearId == dto.FiscalYearId);

        if (fiscalPeriod == null)
            throw new ArgumentException(
                "Fiscal year period not found or does not belong to the selected fiscal year.");

        // -----------------------------------------------------
        // Date validation
        // -----------------------------------------------------

        if (dto.OrderDate.Date < fiscalPeriod.StartDate.Date ||
            dto.OrderDate.Date > fiscalPeriod.EndDate.Date)
        {
            throw new ArgumentException(
                "Order date must be within the selected fiscal year period.");
        }

        // -----------------------------------------------------
        // Calculate
        // -----------------------------------------------------

        decimal subTotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        var lines = new List<SalesOrderLine>();

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            var calculation = CalculateLine(lineDto);

            subTotal += calculation.GrossAmount;
            totalDiscount += calculation.DiscountAmount;
            totalTax += calculation.TaxAmount;

            lines.Add(new SalesOrderLine
            {
                ProductId = lineDto.ProductId,
                UnitId = lineDto.UnitId,
                Description = lineDto.Description,

                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,

                DiscountPercent = lineDto.DiscountPercent,
                DiscountAmount = calculation.DiscountAmount,

                TaxPercent = lineDto.TaxPercent,
                TaxAmount = calculation.TaxAmount,

                LineTotal = calculation.LineTotal,

                LineNumber = lineDto.LineNumber
            });
        }

        decimal grandTotal =
            subTotal - totalDiscount + totalTax;

        // -----------------------------------------------------
        // Create Order
        // -----------------------------------------------------

        var order = new SalesOrder
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,

            CustomerId = dto.CustomerId,

            SalesQuotationId = dto.SalesQuotationId,

            OrderNumber = dto.OrderNumber,
            OrderDate = dto.OrderDate,

            ExpectedDeliveryDate =
                dto.ExpectedDeliveryDate,

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
            CreatedAt = DateTime.UtcNow,

            SalesOrderLines = lines
        };

        _context.SalesOrders.Add(order);

        await _context.SaveChangesAsync();

        return MapToDto(order);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        SalesOrderDto dto)
    {
        var order = await _context.SalesOrders
            .Include(x => x.SalesOrderLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (order == null)
            return false;

        if (!string.Equals(
                order.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft sales orders can be updated.");
        }

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one sales order line is required.");

        if (dto.ExchangeRate <= 0)
            throw new ArgumentException(
                "Exchange rate must be greater than zero.");

        // -----------------------------------------------------
        // Calculate
        // -----------------------------------------------------

        decimal subTotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            var calculation = CalculateLine(lineDto);

            subTotal += calculation.GrossAmount;
            totalDiscount += calculation.DiscountAmount;
            totalTax += calculation.TaxAmount;
        }

        decimal grandTotal =
            subTotal - totalDiscount + totalTax;

        // -----------------------------------------------------
        // Update Header
        // -----------------------------------------------------

        order.CustomerId = dto.CustomerId;

        order.SalesQuotationId =
            dto.SalesQuotationId;

        order.OrderDate =
            dto.OrderDate;

        order.ExpectedDeliveryDate =
            dto.ExpectedDeliveryDate;

        order.CurrencyId =
            dto.CurrencyId;

        order.ExchangeRate =
            dto.ExchangeRate;

        order.ReferenceNumber =
            dto.ReferenceNumber;

        order.Notes =
            dto.Notes;

        order.BranchId =
            dto.BranchId;

        order.WarehouseId =
            dto.WarehouseId;

        order.Status =
            dto.Status;

        order.SubTotal =
            subTotal;

        order.DiscountAmount =
            totalDiscount;

        order.TaxAmount =
            totalTax;

        order.GrandTotal =
            grandTotal;

        order.UpdatedAt =
            DateTime.UtcNow;

        // -----------------------------------------------------
        // Replace Lines
        // -----------------------------------------------------

        _context.SalesOrderLines.RemoveRange(
            order.SalesOrderLines);

        foreach (var lineDto in dto.Lines
                     .OrderBy(x => x.LineNumber))
        {
            var calculation =
                CalculateLine(lineDto);

            order.SalesOrderLines.Add(
                new SalesOrderLine
                {
                    SalesOrderId = order.Id,

                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,

                    Description = lineDto.Description,

                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,

                    DiscountPercent =
                        lineDto.DiscountPercent,

                    DiscountAmount =
                        calculation.DiscountAmount,

                    TaxPercent =
                        lineDto.TaxPercent,

                    TaxAmount =
                        calculation.TaxAmount,

                    LineTotal =
                        calculation.LineTotal,

                    LineNumber =
                        lineDto.LineNumber
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _context.SalesOrders
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (order == null)
            return false;

        order.IsActive = false;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // VALIDATE LINE
    // =========================================================

    private static void ValidateLine(
        SalesOrderLineDto line)
    {
        if (line.ProductId <= 0)
            throw new ArgumentException(
                $"Invalid ProductId on line {line.LineNumber}.");

        if (line.Quantity <= 0)
            throw new ArgumentException(
                $"Quantity must be greater than zero on line {line.LineNumber}.");

        if (line.UnitPrice < 0)
            throw new ArgumentException(
                $"Unit price cannot be negative on line {line.LineNumber}.");

        if (line.DiscountPercent < 0 ||
            line.DiscountPercent > 100)
        {
            throw new ArgumentException(
                $"Discount percent must be between 0 and 100 on line {line.LineNumber}.");
        }

        if (line.TaxPercent < 0 ||
            line.TaxPercent > 100)
        {
            throw new ArgumentException(
                $"Tax percent must be between 0 and 100 on line {line.LineNumber}.");
        }
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
    ) CalculateLine(
        SalesOrderLineDto line)
    {
        decimal grossAmount =
            line.Quantity * line.UnitPrice;

        decimal discountAmount =
            grossAmount *
            line.DiscountPercent / 100m;

        decimal taxableAmount =
            grossAmount - discountAmount;

        decimal taxAmount =
            taxableAmount *
            line.TaxPercent / 100m;

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
    // MAP ENTITY → DTO
    // =========================================================

    private static SalesOrderDto MapToDto(
        SalesOrder order)
    {
        return new SalesOrderDto
        {
            Id = order.Id,

            FiscalYearId =
                order.FiscalYearId,

            FiscalYearPeriodId =
                order.FiscalYearPeriodId,

            CustomerId =
                order.CustomerId,

            SalesQuotationId =
                order.SalesQuotationId,

            OrderNumber =
                order.OrderNumber,

            OrderDate =
                order.OrderDate,

            ExpectedDeliveryDate =
                order.ExpectedDeliveryDate,

            CurrencyId =
                order.CurrencyId,

            ExchangeRate =
                order.ExchangeRate,

            SubTotal =
                order.SubTotal,

            DiscountAmount =
                order.DiscountAmount,

            TaxAmount =
                order.TaxAmount,

            GrandTotal =
                order.GrandTotal,

            Status =
                order.Status,

            ReferenceNumber =
                order.ReferenceNumber,

            Notes =
                order.Notes,

            BranchId =
                order.BranchId,

            WarehouseId =
                order.WarehouseId,

            IsActive =
                order.IsActive,

            CreatedAt =
                order.CreatedAt,

            UpdatedAt =
                order.UpdatedAt,

            Lines = order.SalesOrderLines
                .OrderBy(x => x.LineNumber)
                .Select(x => new SalesOrderLineDto
                {
                    Id = x.Id,

                    SalesOrderId =
                        x.SalesOrderId,

                    ProductId =
                        x.ProductId,

                    UnitId =
                        x.UnitId,

                    Description =
                        x.Description,

                    Quantity =
                        x.Quantity,

                    UnitPrice =
                        x.UnitPrice,

                    DiscountPercent =
                        x.DiscountPercent,

                    DiscountAmount =
                        x.DiscountAmount,

                    TaxPercent =
                        x.TaxPercent,

                    TaxAmount =
                        x.TaxAmount,

                    LineTotal =
                        x.LineTotal,

                    LineNumber =
                        x.LineNumber
                })
                .ToList()
        };
    }
}
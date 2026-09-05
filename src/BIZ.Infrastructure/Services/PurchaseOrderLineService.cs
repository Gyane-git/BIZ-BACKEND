using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseOrderLineService
    : IPurchaseOrderLineService
{
    private readonly TenantDbContext _context;

    public PurchaseOrderLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseOrderLineDto>> GetAllAsync()
    {
        return await _context.PurchaseOrderLines
            .AsNoTracking()
            .Where(x => x.PurchaseOrder.IsActive)
            .OrderBy(x => x.PurchaseOrderId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new PurchaseOrderLineDto
            {
                Id = x.Id,
                PurchaseOrderId = x.PurchaseOrderId,
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

    public async Task<PurchaseOrderLineDto?> GetByIdAsync(int id)
    {
        return await _context.PurchaseOrderLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.PurchaseOrder.IsActive)
            .Select(x => new PurchaseOrderLineDto
            {
                Id = x.Id,
                PurchaseOrderId = x.PurchaseOrderId,
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

    public async Task<PurchaseOrderLineDto> CreateAsync(
        PurchaseOrderLineDto dto)
    {
        var order = await _context.PurchaseOrders
            .FirstOrDefaultAsync(x =>
                x.Id == dto.PurchaseOrderId &&
                x.IsActive);

        if (order == null)
            throw new ArgumentException(
                "PurchaseOrder not found.");

        if (!order.Status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be added to Draft PurchaseOrder.");
        }

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "Valid ProductId is required.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitPrice < 0)
            throw new ArgumentException(
                "UnitPrice cannot be negative.");

        if (dto.DiscountPercent < 0 ||
            dto.DiscountPercent > 100)
        {
            throw new ArgumentException(
                "DiscountPercent must be between 0 and 100.");
        }

        if (dto.TaxPercent < 0 ||
            dto.TaxPercent > 100)
        {
            throw new ArgumentException(
                "TaxPercent must be between 0 and 100.");
        }

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

        var duplicate = await _context.PurchaseOrderLines
            .AnyAsync(x =>
                x.PurchaseOrderId == dto.PurchaseOrderId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                $"LineNumber {dto.LineNumber} already exists.");

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

        var line = new PurchaseOrderLine
        {
            PurchaseOrderId = dto.PurchaseOrderId,
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

        _context.PurchaseOrderLines.Add(line);

        await _context.SaveChangesAsync();

        return MapToDto(line);
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseOrderLineDto dto)
    {
        var line = await _context.PurchaseOrderLines
            .Include(x => x.PurchaseOrder)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.PurchaseOrder.IsActive);

        if (line == null)
            return false;

        if (!line.PurchaseOrder.Status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft PurchaseOrder lines can be updated.");
        }

        if (dto.ProductId <= 0)
            throw new ArgumentException(
                "Valid ProductId is required.");

        if (dto.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (dto.UnitPrice < 0)
            throw new ArgumentException(
                "UnitPrice cannot be negative.");

        if (dto.DiscountPercent < 0 ||
            dto.DiscountPercent > 100)
        {
            throw new ArgumentException(
                "DiscountPercent must be between 0 and 100.");
        }

        if (dto.TaxPercent < 0 ||
            dto.TaxPercent > 100)
        {
            throw new ArgumentException(
                "TaxPercent must be between 0 and 100.");
        }

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

        var duplicate = await _context.PurchaseOrderLines
            .AnyAsync(x =>
                x.Id != id &&
                x.PurchaseOrderId ==
                    line.PurchaseOrderId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                $"LineNumber {dto.LineNumber} already exists.");

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
        var line = await _context.PurchaseOrderLines
            .Include(x => x.PurchaseOrder)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.PurchaseOrder.IsActive);

        if (line == null)
            return false;

        if (!line.PurchaseOrder.Status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft PurchaseOrder lines can be deleted.");
        }

        _context.PurchaseOrderLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }

    private static PurchaseOrderLineDto MapToDto(
        PurchaseOrderLine x)
    {
        return new PurchaseOrderLineDto
        {
            Id = x.Id,
            PurchaseOrderId = x.PurchaseOrderId,
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
        };
    }
}
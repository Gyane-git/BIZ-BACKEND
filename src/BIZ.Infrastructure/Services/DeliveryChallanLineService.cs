using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class DeliveryChallanLineService
    : IDeliveryChallanLineService
{
    private readonly TenantDbContext _context;

    public DeliveryChallanLineService(
        TenantDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<IEnumerable<DeliveryChallanLineDto>>
        GetAllAsync()
    {
        var lines =
            await _context.DeliveryChallanLines
                .Include(x => x.DeliveryChallan)
                .Where(x =>
                    x.DeliveryChallan.IsActive)
                .OrderBy(x => x.DeliveryChallanId)
                .ThenBy(x => x.LineNumber)
                .ToListAsync();

        return lines.Select(MapToDto);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<DeliveryChallanLineDto?>
        GetByIdAsync(int id)
    {
        var line =
            await _context.DeliveryChallanLines
                .Include(x => x.DeliveryChallan)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.DeliveryChallan.IsActive);

        if (line == null)
            return null;

        return MapToDto(line);
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<DeliveryChallanLineDto>
        CreateAsync(
            DeliveryChallanLineDto dto)
    {
        var challan =
            await _context.DeliveryChallans
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.DeliveryChallanId &&
                    x.IsActive);

        if (challan == null)
            throw new ArgumentException(
                "Delivery challan not found.");

        if (!string.Equals(
                challan.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be added to a Draft delivery challan.");
        }

        ValidateLine(dto);

        var duplicate =
            await _context.DeliveryChallanLines
                .AnyAsync(x =>
                    x.DeliveryChallanId ==
                        dto.DeliveryChallanId &&
                    x.LineNumber ==
                        dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this challan.");

        var line =
            new DeliveryChallanLine
            {
                DeliveryChallanId =
                    dto.DeliveryChallanId,

                ProductId =
                    dto.ProductId,

                UnitId =
                    dto.UnitId,

                Description =
                    dto.Description,

                Quantity =
                    dto.Quantity,

                LineNumber =
                    dto.LineNumber
            };

        _context.DeliveryChallanLines.Add(line);

        await _context.SaveChangesAsync();

        return MapToDto(line);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        DeliveryChallanLineDto dto)
    {
        var line =
            await _context.DeliveryChallanLines
                .Include(x => x.DeliveryChallan)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!string.Equals(
                line.DeliveryChallan.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be updated when the delivery challan is Draft.");
        }

        ValidateLine(dto);

        var duplicate =
            await _context.DeliveryChallanLines
                .AnyAsync(x =>
                    x.Id != id &&
                    x.DeliveryChallanId ==
                        line.DeliveryChallanId &&
                    x.LineNumber ==
                        dto.LineNumber);

        if (duplicate)
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this challan.");

        line.ProductId =
            dto.ProductId;

        line.UnitId =
            dto.UnitId;

        line.Description =
            dto.Description;

        line.Quantity =
            dto.Quantity;

        line.LineNumber =
            dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var line =
            await _context.DeliveryChallanLines
                .Include(x => x.DeliveryChallan)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!string.Equals(
                line.DeliveryChallan.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be deleted when the delivery challan is Draft.");
        }

        _context.DeliveryChallanLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // VALIDATION
    // =========================================================

    private static void ValidateLine(
        DeliveryChallanLineDto line)
    {
        if (line.DeliveryChallanId <= 0)
            throw new ArgumentException(
                "Delivery challan ID is required.");

        if (line.ProductId <= 0)
            throw new ArgumentException(
                "Product ID is required.");

        if (line.Quantity <= 0)
            throw new ArgumentException(
                "Quantity must be greater than zero.");

        if (line.LineNumber <= 0)
            throw new ArgumentException(
                "Line number must be greater than zero.");
    }

    // =========================================================
    // MAP
    // =========================================================

    private static DeliveryChallanLineDto MapToDto(
        DeliveryChallanLine line)
    {
        return new DeliveryChallanLineDto
        {
            Id =
                line.Id,

            DeliveryChallanId =
                line.DeliveryChallanId,

            ProductId =
                line.ProductId,

            UnitId =
                line.UnitId,

            Description =
                line.Description,

            Quantity =
                line.Quantity,

            LineNumber =
                line.LineNumber
        };
    }
}
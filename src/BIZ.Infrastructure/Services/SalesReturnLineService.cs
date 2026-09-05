using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesReturnLineService : ISalesReturnLineService
{
    private readonly TenantDbContext _context;

    public SalesReturnLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesReturnLineDto>> GetAllAsync()
    {
        var lines = await _context.SalesReturnLines
            .Include(x => x.SalesReturn)
            .Where(x => x.SalesReturn.IsActive)
            .OrderBy(x => x.SalesReturnId)
            .ThenBy(x => x.LineNumber)
            .ToListAsync();

        return lines.Select(MapToDto);
    }

    public async Task<SalesReturnLineDto?> GetByIdAsync(
        int id)
    {
        var line = await _context.SalesReturnLines
            .Include(x => x.SalesReturn)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.SalesReturn.IsActive);

        if (line == null)
            return null;

        return MapToDto(line);
    }

    public async Task<SalesReturnLineDto> CreateAsync(
        SalesReturnLineDto dto)
    {
        var salesReturn =
            await _context.SalesReturns
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.SalesReturnId &&
                    x.IsActive);

        if (salesReturn == null)
        {
            throw new ArgumentException(
                "Sales return not found.");
        }

        if (!string.Equals(
                salesReturn.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be added to a Draft sales return.");
        }

        ValidateLine(dto);

        var duplicate =
            await _context.SalesReturnLines
                .AnyAsync(x =>
                    x.SalesReturnId ==
                        dto.SalesReturnId &&
                    x.LineNumber ==
                        dto.LineNumber);

        if (duplicate)
        {
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this return.");
        }

        var grossAmount =
            dto.Quantity * dto.UnitPrice;

        var discountAmount =
            grossAmount *
            dto.DiscountPercent / 100m;

        var taxableAmount =
            grossAmount -
            discountAmount;

        var taxAmount =
            taxableAmount *
            dto.TaxPercent / 100m;

        var lineTotal =
            taxableAmount +
            taxAmount;

        var line = new SalesReturnLine
        {
            SalesReturnId =
                dto.SalesReturnId,

            ProductId =
                dto.ProductId,

            UnitId =
                dto.UnitId,

            Description =
                dto.Description,

            Quantity =
                dto.Quantity,

            UnitPrice =
                dto.UnitPrice,

            DiscountPercent =
                dto.DiscountPercent,

            DiscountAmount =
                discountAmount,

            TaxPercent =
                dto.TaxPercent,

            TaxAmount =
                taxAmount,

            LineTotal =
                lineTotal,

            LineNumber =
                dto.LineNumber
        };

        _context.SalesReturnLines.Add(line);

        await _context.SaveChangesAsync();

        return MapToDto(line);
    }

    public async Task<bool> UpdateAsync(
        int id,
        SalesReturnLineDto dto)
    {
        var line =
            await _context.SalesReturnLines
                .Include(x => x.SalesReturn)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!string.Equals(
                line.SalesReturn.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be updated when the sales return is Draft.");
        }

        if (dto.SalesReturnId != line.SalesReturnId)
        {
            throw new ArgumentException(
                "Sales return ID cannot be changed.");
        }

        ValidateLine(dto);

        var duplicate =
            await _context.SalesReturnLines
                .AnyAsync(x =>
                    x.Id != id &&
                    x.SalesReturnId ==
                        line.SalesReturnId &&
                    x.LineNumber ==
                        dto.LineNumber);

        if (duplicate)
        {
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this return.");
        }

        var grossAmount =
            dto.Quantity * dto.UnitPrice;

        var discountAmount =
            grossAmount *
            dto.DiscountPercent / 100m;

        var taxableAmount =
            grossAmount -
            discountAmount;

        var taxAmount =
            taxableAmount *
            dto.TaxPercent / 100m;

        var lineTotal =
            taxableAmount +
            taxAmount;

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
            discountAmount;

        line.TaxPercent =
            dto.TaxPercent;

        line.TaxAmount =
            taxAmount;

        line.LineTotal =
            lineTotal;

        line.LineNumber =
            dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line =
            await _context.SalesReturnLines
                .Include(x => x.SalesReturn)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!string.Equals(
                line.SalesReturn.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be deleted when the sales return is Draft.");
        }

        _context.SalesReturnLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }

    private static void ValidateLine(
        SalesReturnLineDto line)
    {
        if (line.SalesReturnId <= 0)
        {
            throw new ArgumentException(
                "Sales return ID is required.");
        }

        if (line.ProductId <= 0)
        {
            throw new ArgumentException(
                "Product ID is required.");
        }

        if (line.Quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        if (line.UnitPrice < 0)
        {
            throw new ArgumentException(
                "Unit price cannot be negative.");
        }

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

        if (line.LineNumber <= 0)
        {
            throw new ArgumentException(
                "Line number must be greater than zero.");
        }
    }

    private static SalesReturnLineDto MapToDto(
        SalesReturnLine line)
    {
        return new SalesReturnLineDto
        {
            Id = line.Id,

            SalesReturnId =
                line.SalesReturnId,

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
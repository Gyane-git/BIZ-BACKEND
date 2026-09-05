using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesInvoiceLineService : ISalesInvoiceLineService
{
    private readonly TenantDbContext _context;

    public SalesInvoiceLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesInvoiceLineDto>> GetAllAsync()
    {
        var lines = await _context.SalesInvoiceLines
            .Include(x => x.SalesInvoice)
            .Where(x => x.SalesInvoice.IsActive)
            .OrderBy(x => x.SalesInvoiceId)
            .ThenBy(x => x.LineNumber)
            .ToListAsync();

        return lines.Select(MapToDto);
    }

    public async Task<SalesInvoiceLineDto?> GetByIdAsync(
        int id)
    {
        var line = await _context.SalesInvoiceLines
            .Include(x => x.SalesInvoice)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.SalesInvoice.IsActive);

        if (line == null)
            return null;

        return MapToDto(line);
    }

    public async Task<SalesInvoiceLineDto> CreateAsync(
        SalesInvoiceLineDto dto)
    {
        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SalesInvoiceId &&
                x.IsActive);

        if (invoice == null)
            throw new ArgumentException(
                "Sales invoice not found.");

        if (!string.Equals(
                invoice.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be added to a Draft sales invoice.");
        }

        ValidateLine(dto);

        var duplicate = await _context.SalesInvoiceLines
            .AnyAsync(x =>
                x.SalesInvoiceId == dto.SalesInvoiceId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
        {
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this invoice.");
        }

        var grossAmount =
            dto.Quantity * dto.UnitPrice;

        var discountAmount =
            grossAmount * dto.DiscountPercent / 100m;

        var taxableAmount =
            grossAmount - discountAmount;

        var taxAmount =
            taxableAmount * dto.TaxPercent / 100m;

        var lineTotal =
            taxableAmount + taxAmount;

        var line = new SalesInvoiceLine
        {
            SalesInvoiceId = dto.SalesInvoiceId,

            ProductId = dto.ProductId,
            UnitId = dto.UnitId,
            Description = dto.Description,

            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,

            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = discountAmount,

            TaxPercent = dto.TaxPercent,
            TaxAmount = taxAmount,

            LineTotal = lineTotal,

            LineNumber = dto.LineNumber
        };

        _context.SalesInvoiceLines.Add(line);

        await _context.SaveChangesAsync();

        return MapToDto(line);
    }

    public async Task<bool> UpdateAsync(
        int id,
        SalesInvoiceLineDto dto)
    {
        var line = await _context.SalesInvoiceLines
            .Include(x => x.SalesInvoice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!string.Equals(
                line.SalesInvoice.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be updated when the sales invoice is Draft.");
        }

        ValidateLine(dto);

        if (dto.SalesInvoiceId != line.SalesInvoiceId)
        {
            throw new ArgumentException(
                "Sales invoice ID cannot be changed.");
        }

        var duplicate = await _context.SalesInvoiceLines
            .AnyAsync(x =>
                x.Id != id &&
                x.SalesInvoiceId == line.SalesInvoiceId &&
                x.LineNumber == dto.LineNumber);

        if (duplicate)
        {
            throw new ArgumentException(
                $"Line number {dto.LineNumber} already exists in this invoice.");
        }

        var grossAmount =
            dto.Quantity * dto.UnitPrice;

        var discountAmount =
            grossAmount * dto.DiscountPercent / 100m;

        var taxableAmount =
            grossAmount - discountAmount;

        var taxAmount =
            taxableAmount * dto.TaxPercent / 100m;

        var lineTotal =
            taxableAmount + taxAmount;

        line.ProductId = dto.ProductId;
        line.UnitId = dto.UnitId;
        line.Description = dto.Description;

        line.Quantity = dto.Quantity;
        line.UnitPrice = dto.UnitPrice;

        line.DiscountPercent = dto.DiscountPercent;
        line.DiscountAmount = discountAmount;

        line.TaxPercent = dto.TaxPercent;
        line.TaxAmount = taxAmount;

        line.LineTotal = lineTotal;

        line.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var line = await _context.SalesInvoiceLines
            .Include(x => x.SalesInvoice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (line == null)
            return false;

        if (!string.Equals(
                line.SalesInvoice.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Lines can only be deleted when the sales invoice is Draft.");
        }

        _context.SalesInvoiceLines.Remove(line);

        await _context.SaveChangesAsync();

        return true;
    }

    private static void ValidateLine(
        SalesInvoiceLineDto line)
    {
        if (line.SalesInvoiceId <= 0)
        {
            throw new ArgumentException(
                "Sales invoice ID is required.");
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

    private static SalesInvoiceLineDto MapToDto(
        SalesInvoiceLine line)
    {
        return new SalesInvoiceLineDto
        {
            Id = line.Id,

            SalesInvoiceId = line.SalesInvoiceId,

            ProductId = line.ProductId,
            UnitId = line.UnitId,

            Description = line.Description,

            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,

            DiscountPercent = line.DiscountPercent,
            DiscountAmount = line.DiscountAmount,

            TaxPercent = line.TaxPercent,
            TaxAmount = line.TaxAmount,

            LineTotal = line.LineTotal,

            LineNumber = line.LineNumber
        };
    }
}
using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesReturnService : ISalesReturnService
{
    private readonly TenantDbContext _context;

    public SalesReturnService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesReturnDto>> GetAllAsync()
    {
        var returns = await _context.SalesReturns
            .Include(x => x.SalesReturnLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return returns.Select(MapToDto);
    }

    public async Task<SalesReturnDto?> GetByIdAsync(int id)
    {
        var salesReturn = await _context.SalesReturns
            .Include(x => x.SalesReturnLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (salesReturn == null)
            return null;

        return MapToDto(salesReturn);
    }

    public async Task<SalesReturnDto> CreateAsync(
        SalesReturnDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ReturnNumber))
            throw new ArgumentException(
                "Return number is required.");

        if (dto.CustomerId <= 0)
            throw new ArgumentException(
                "Customer ID is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one sales return line is required.");

        var returnExists = await _context.SalesReturns
            .AnyAsync(x =>
                x.ReturnNumber == dto.ReturnNumber);

        if (returnExists)
        {
            throw new ArgumentException(
                $"Return number '{dto.ReturnNumber}' already exists.");
        }

        var fiscalYearExists = await _context.FiscalYears
            .AnyAsync(x => x.Id == dto.FiscalYearId);

        if (!fiscalYearExists)
            throw new ArgumentException(
                "Fiscal year not found.");

        var fiscalPeriod = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.FiscalYearId == dto.FiscalYearId);

        if (fiscalPeriod == null)
        {
            throw new ArgumentException(
                "Fiscal year period not found or does not belong to the selected fiscal year.");
        }

        if (dto.ReturnDate.Date < fiscalPeriod.StartDate.Date ||
            dto.ReturnDate.Date > fiscalPeriod.EndDate.Date)
        {
            throw new ArgumentException(
                "Return date must be within the selected fiscal year period.");
        }

        var lines = new List<SalesReturnLine>();

        foreach (var lineDto in dto.Lines
                     .OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            if (lines.Any(x =>
                    x.LineNumber == lineDto.LineNumber))
            {
                throw new ArgumentException(
                    $"Line number {lineDto.LineNumber} is duplicated.");
            }

            var grossAmount =
                lineDto.Quantity * lineDto.UnitPrice;

            var discountAmount =
                grossAmount *
                lineDto.DiscountPercent / 100m;

            var taxableAmount =
                grossAmount - discountAmount;

            var taxAmount =
                taxableAmount *
                lineDto.TaxPercent / 100m;

            var lineTotal =
                taxableAmount + taxAmount;

            lines.Add(new SalesReturnLine
            {
                ProductId = lineDto.ProductId,
                UnitId = lineDto.UnitId,
                Description = lineDto.Description,

                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,

                DiscountPercent =
                    lineDto.DiscountPercent,

                DiscountAmount =
                    discountAmount,

                TaxPercent =
                    lineDto.TaxPercent,

                TaxAmount =
                    taxAmount,

                LineTotal =
                    lineTotal,

                LineNumber =
                    lineDto.LineNumber
            });
        }

        var subTotal = lines.Sum(x =>
            x.Quantity * x.UnitPrice);

        var discountTotal = lines.Sum(x =>
            x.DiscountAmount);

        var taxTotal = lines.Sum(x =>
            x.TaxAmount);

        var grandTotal =
            subTotal -
            discountTotal +
            taxTotal;

        var salesReturn = new SalesReturn
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,

            CustomerId = dto.CustomerId,

            SalesInvoiceId = dto.SalesInvoiceId,
            DeliveryChallanId = dto.DeliveryChallanId,

            ReturnNumber = dto.ReturnNumber,

            ReturnDate = dto.ReturnDate,

            SubTotal = subTotal,
            DiscountAmount = discountTotal,
            TaxAmount = taxTotal,
            GrandTotal = grandTotal,

            Status = "Draft",

            Reason = dto.Reason,

            ReferenceNumber =
                dto.ReferenceNumber,

            Notes = dto.Notes,

            BranchId = dto.BranchId,
            WarehouseId = dto.WarehouseId,

            IsActive = true,

            CreatedAt = DateTime.UtcNow,

            SalesReturnLines = lines
        };

        _context.SalesReturns.Add(salesReturn);

        await _context.SaveChangesAsync();

        return MapToDto(salesReturn);
    }

    public async Task<bool> UpdateAsync(
        int id,
        SalesReturnDto dto)
    {
        var salesReturn = await _context.SalesReturns
            .Include(x => x.SalesReturnLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (salesReturn == null)
            return false;

        if (!string.Equals(
                salesReturn.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft sales returns can be updated.");
        }

        if (dto.Lines == null || dto.Lines.Count == 0)
        {
            throw new ArgumentException(
                "At least one sales return line is required.");
        }

        var duplicateReturnNumber =
            await _context.SalesReturns.AnyAsync(x =>
                x.Id != id &&
                x.ReturnNumber == dto.ReturnNumber);

        if (duplicateReturnNumber)
        {
            throw new ArgumentException(
                $"Return number '{dto.ReturnNumber}' already exists.");
        }

        var lineNumbers = new HashSet<int>();

        var newLines = new List<SalesReturnLine>();

        foreach (var lineDto in dto.Lines
                     .OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            if (!lineNumbers.Add(lineDto.LineNumber))
            {
                throw new ArgumentException(
                    $"Line number {lineDto.LineNumber} is duplicated.");
            }

            var grossAmount =
                lineDto.Quantity *
                lineDto.UnitPrice;

            var discountAmount =
                grossAmount *
                lineDto.DiscountPercent / 100m;

            var taxableAmount =
                grossAmount -
                discountAmount;

            var taxAmount =
                taxableAmount *
                lineDto.TaxPercent / 100m;

            var lineTotal =
                taxableAmount +
                taxAmount;

            newLines.Add(new SalesReturnLine
            {
                SalesReturnId = salesReturn.Id,

                ProductId = lineDto.ProductId,
                UnitId = lineDto.UnitId,
                Description = lineDto.Description,

                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,

                DiscountPercent =
                    lineDto.DiscountPercent,

                DiscountAmount =
                    discountAmount,

                TaxPercent =
                    lineDto.TaxPercent,

                TaxAmount =
                    taxAmount,

                LineTotal =
                    lineTotal,

                LineNumber =
                    lineDto.LineNumber
            });
        }

        var subTotal = newLines.Sum(x =>
            x.Quantity * x.UnitPrice);

        var discountTotal = newLines.Sum(x =>
            x.DiscountAmount);

        var taxTotal = newLines.Sum(x =>
            x.TaxAmount);

        var grandTotal =
            subTotal -
            discountTotal +
            taxTotal;

        salesReturn.FiscalYearId =
            dto.FiscalYearId;

        salesReturn.FiscalYearPeriodId =
            dto.FiscalYearPeriodId;

        salesReturn.CustomerId =
            dto.CustomerId;

        salesReturn.SalesInvoiceId =
            dto.SalesInvoiceId;

        salesReturn.DeliveryChallanId =
            dto.DeliveryChallanId;

        salesReturn.ReturnNumber =
            dto.ReturnNumber;

        salesReturn.ReturnDate =
            dto.ReturnDate;

        salesReturn.SubTotal =
            subTotal;

        salesReturn.DiscountAmount =
            discountTotal;

        salesReturn.TaxAmount =
            taxTotal;

        salesReturn.GrandTotal =
            grandTotal;

        salesReturn.Reason =
            dto.Reason;

        salesReturn.ReferenceNumber =
            dto.ReferenceNumber;

        salesReturn.Notes =
            dto.Notes;

        salesReturn.BranchId =
            dto.BranchId;

        salesReturn.WarehouseId =
            dto.WarehouseId;

        salesReturn.UpdatedAt =
            DateTime.UtcNow;

        _context.SalesReturnLines
            .RemoveRange(
                salesReturn.SalesReturnLines);

        foreach (var line in newLines)
        {
            salesReturn.SalesReturnLines.Add(line);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var salesReturn = await _context.SalesReturns
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (salesReturn == null)
            return false;

        if (!string.Equals(
                salesReturn.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft sales returns can be deleted.");
        }

        salesReturn.IsActive = false;

        salesReturn.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static void ValidateLine(
        SalesReturnLineDto line)
    {
        if (line.ProductId <= 0)
        {
            throw new ArgumentException(
                $"Invalid ProductId on line {line.LineNumber}.");
        }

        if (line.Quantity <= 0)
        {
            throw new ArgumentException(
                $"Quantity must be greater than zero on line {line.LineNumber}.");
        }

        if (line.UnitPrice < 0)
        {
            throw new ArgumentException(
                $"Unit price cannot be negative on line {line.LineNumber}.");
        }

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

        if (line.LineNumber <= 0)
        {
            throw new ArgumentException(
                "Line number must be greater than zero.");
        }
    }

    private static SalesReturnDto MapToDto(
        SalesReturn salesReturn)
    {
        return new SalesReturnDto
        {
            Id = salesReturn.Id,

            FiscalYearId =
                salesReturn.FiscalYearId,

            FiscalYearPeriodId =
                salesReturn.FiscalYearPeriodId,

            CustomerId =
                salesReturn.CustomerId,

            SalesInvoiceId =
                salesReturn.SalesInvoiceId,

            DeliveryChallanId =
                salesReturn.DeliveryChallanId,

            ReturnNumber =
                salesReturn.ReturnNumber,

            ReturnDate =
                salesReturn.ReturnDate,

            SubTotal =
                salesReturn.SubTotal,

            DiscountAmount =
                salesReturn.DiscountAmount,

            TaxAmount =
                salesReturn.TaxAmount,

            GrandTotal =
                salesReturn.GrandTotal,

            Status =
                salesReturn.Status,

            Reason =
                salesReturn.Reason,

            ReferenceNumber =
                salesReturn.ReferenceNumber,

            Notes =
                salesReturn.Notes,

            BranchId =
                salesReturn.BranchId,

            WarehouseId =
                salesReturn.WarehouseId,

            IsActive =
                salesReturn.IsActive,

            CreatedAt =
                salesReturn.CreatedAt,

            UpdatedAt =
                salesReturn.UpdatedAt,

            Lines = salesReturn.SalesReturnLines
                .OrderBy(x => x.LineNumber)
                .Select(x => new SalesReturnLineDto
                {
                    Id = x.Id,

                    SalesReturnId =
                        x.SalesReturnId,

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
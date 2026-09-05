using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesInvoiceService : ISalesInvoiceService
{
    private readonly TenantDbContext _context;

    public SalesInvoiceService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetAllAsync()
    {
        var invoices = await _context.SalesInvoices
            .Include(x => x.SalesInvoiceLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return invoices.Select(MapToDto);
    }

    public async Task<SalesInvoiceDto?> GetByIdAsync(int id)
    {
        var invoice = await _context.SalesInvoices
            .Include(x => x.SalesInvoiceLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (invoice == null)
            return null;

        return MapToDto(invoice);
    }

    public async Task<SalesInvoiceDto> CreateAsync(SalesInvoiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.InvoiceNumber))
            throw new ArgumentException("Invoice number is required.");

        if (dto.CustomerId <= 0)
            throw new ArgumentException("Customer ID is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new ArgumentException(
                "At least one sales invoice line is required.");

        if (dto.ExchangeRate <= 0)
            throw new ArgumentException(
                "Exchange rate must be greater than zero.");

        var invoiceExists = await _context.SalesInvoices
            .AnyAsync(x => x.InvoiceNumber == dto.InvoiceNumber);

        if (invoiceExists)
            throw new ArgumentException(
                $"Invoice number '{dto.InvoiceNumber}' already exists.");

        var fiscalYearExists = await _context.FiscalYears
            .AnyAsync(x => x.Id == dto.FiscalYearId);

        if (!fiscalYearExists)
            throw new ArgumentException("Fiscal year not found.");

        var fiscalPeriod = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.FiscalYearId == dto.FiscalYearId);

        if (fiscalPeriod == null)
            throw new ArgumentException(
                "Fiscal year period not found or does not belong to the selected fiscal year.");

        if (dto.InvoiceDate.Date < fiscalPeriod.StartDate.Date ||
            dto.InvoiceDate.Date > fiscalPeriod.EndDate.Date)
        {
            throw new ArgumentException(
                "Invoice date must be within the selected fiscal year period.");
        }

        if (dto.DueDate.HasValue &&
            dto.DueDate.Value.Date < dto.InvoiceDate.Date)
        {
            throw new ArgumentException(
                "Due date cannot be earlier than invoice date.");
        }

        var lines = new List<SalesInvoiceLine>();

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            if (lines.Any(x => x.LineNumber == lineDto.LineNumber))
            {
                throw new ArgumentException(
                    $"Line number {lineDto.LineNumber} is duplicated.");
            }

            var grossAmount =
                lineDto.Quantity * lineDto.UnitPrice;

            var discountAmount =
                grossAmount * lineDto.DiscountPercent / 100m;

            var taxableAmount =
                grossAmount - discountAmount;

            var taxAmount =
                taxableAmount * lineDto.TaxPercent / 100m;

            var lineTotal =
                taxableAmount + taxAmount;

            lines.Add(new SalesInvoiceLine
            {
                ProductId = lineDto.ProductId,
                UnitId = lineDto.UnitId,
                Description = lineDto.Description,

                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,

                DiscountPercent = lineDto.DiscountPercent,
                DiscountAmount = discountAmount,

                TaxPercent = lineDto.TaxPercent,
                TaxAmount = taxAmount,

                LineTotal = lineTotal,
                LineNumber = lineDto.LineNumber
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

        var paidAmount = 0m;

        var balanceAmount =
            grandTotal - paidAmount;

        var invoice = new SalesInvoice
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,

            CustomerId = dto.CustomerId,

            SalesOrderId = dto.SalesOrderId,
            DeliveryChallanId = dto.DeliveryChallanId,

            InvoiceNumber = dto.InvoiceNumber,

            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,

            CurrencyId = dto.CurrencyId,
            ExchangeRate = dto.ExchangeRate,

            SubTotal = subTotal,
            DiscountAmount = discountTotal,
            TaxAmount = taxTotal,
            GrandTotal = grandTotal,

            PaidAmount = paidAmount,
            BalanceAmount = balanceAmount,

            Status = "Draft",

            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,

            BranchId = dto.BranchId,
            WarehouseId = dto.WarehouseId,

            IsActive = true,

            CreatedAt = DateTime.UtcNow,

            SalesInvoiceLines = lines
        };

        _context.SalesInvoices.Add(invoice);

        await _context.SaveChangesAsync();

        return MapToDto(invoice);
    }

    public async Task<bool> UpdateAsync(
        int id,
        SalesInvoiceDto dto)
    {
        var invoice = await _context.SalesInvoices
            .Include(x => x.SalesInvoiceLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (invoice == null)
            return false;

        if (!string.Equals(
                invoice.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft sales invoices can be updated.");
        }

        if (dto.Lines == null || dto.Lines.Count == 0)
        {
            throw new ArgumentException(
                "At least one sales invoice line is required.");
        }

        if (dto.ExchangeRate <= 0)
        {
            throw new ArgumentException(
                "Exchange rate must be greater than zero.");
        }

        if (dto.DueDate.HasValue &&
            dto.DueDate.Value.Date < dto.InvoiceDate.Date)
        {
            throw new ArgumentException(
                "Due date cannot be earlier than invoice date.");
        }

        var duplicateInvoiceNumber = await _context.SalesInvoices
            .AnyAsync(x =>
                x.Id != id &&
                x.InvoiceNumber == dto.InvoiceNumber);

        if (duplicateInvoiceNumber)
        {
            throw new ArgumentException(
                $"Invoice number '{dto.InvoiceNumber}' already exists.");
        }

        var lineNumbers = new HashSet<int>();

        var newLines = new List<SalesInvoiceLine>();

        foreach (var lineDto in dto.Lines.OrderBy(x => x.LineNumber))
        {
            ValidateLine(lineDto);

            if (!lineNumbers.Add(lineDto.LineNumber))
            {
                throw new ArgumentException(
                    $"Line number {lineDto.LineNumber} is duplicated.");
            }

            var grossAmount =
                lineDto.Quantity * lineDto.UnitPrice;

            var discountAmount =
                grossAmount * lineDto.DiscountPercent / 100m;

            var taxableAmount =
                grossAmount - discountAmount;

            var taxAmount =
                taxableAmount * lineDto.TaxPercent / 100m;

            var lineTotal =
                taxableAmount + taxAmount;

            newLines.Add(new SalesInvoiceLine
            {
                SalesInvoiceId = invoice.Id,

                ProductId = lineDto.ProductId,
                UnitId = lineDto.UnitId,
                Description = lineDto.Description,

                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,

                DiscountPercent = lineDto.DiscountPercent,
                DiscountAmount = discountAmount,

                TaxPercent = lineDto.TaxPercent,
                TaxAmount = taxAmount,

                LineTotal = lineTotal,

                LineNumber = lineDto.LineNumber
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

        invoice.CustomerId = dto.CustomerId;

        invoice.SalesOrderId = dto.SalesOrderId;
        invoice.DeliveryChallanId = dto.DeliveryChallanId;

        invoice.InvoiceNumber = dto.InvoiceNumber;

        invoice.InvoiceDate = dto.InvoiceDate;
        invoice.DueDate = dto.DueDate;

        invoice.CurrencyId = dto.CurrencyId;
        invoice.ExchangeRate = dto.ExchangeRate;

        invoice.SubTotal = subTotal;
        invoice.DiscountAmount = discountTotal;
        invoice.TaxAmount = taxTotal;
        invoice.GrandTotal = grandTotal;

        invoice.PaidAmount = 0m;
        invoice.BalanceAmount = grandTotal;

        invoice.ReferenceNumber = dto.ReferenceNumber;
        invoice.Notes = dto.Notes;

        invoice.BranchId = dto.BranchId;
        invoice.WarehouseId = dto.WarehouseId;

        invoice.UpdatedAt = DateTime.UtcNow;

        _context.SalesInvoiceLines
            .RemoveRange(invoice.SalesInvoiceLines);

        foreach (var line in newLines)
        {
            invoice.SalesInvoiceLines.Add(line);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (invoice == null)
            return false;

        if (!string.Equals(
                invoice.Status,
                "Draft",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only Draft sales invoices can be deleted.");
        }

        invoice.IsActive = false;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static void ValidateLine(
        SalesInvoiceLineDto line)
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

    private static SalesInvoiceDto MapToDto(
        SalesInvoice invoice)
    {
        return new SalesInvoiceDto
        {
            Id = invoice.Id,

            FiscalYearId = invoice.FiscalYearId,
            FiscalYearPeriodId = invoice.FiscalYearPeriodId,

            CustomerId = invoice.CustomerId,

            SalesOrderId = invoice.SalesOrderId,
            DeliveryChallanId = invoice.DeliveryChallanId,

            InvoiceNumber = invoice.InvoiceNumber,

            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,

            CurrencyId = invoice.CurrencyId,
            ExchangeRate = invoice.ExchangeRate,

            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            GrandTotal = invoice.GrandTotal,

            PaidAmount = invoice.PaidAmount,
            BalanceAmount = invoice.BalanceAmount,

            Status = invoice.Status,

            ReferenceNumber = invoice.ReferenceNumber,
            Notes = invoice.Notes,

            BranchId = invoice.BranchId,
            WarehouseId = invoice.WarehouseId,

            IsActive = invoice.IsActive,

            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt,

            Lines = invoice.SalesInvoiceLines
                .OrderBy(x => x.LineNumber)
                .Select(x => new SalesInvoiceLineDto
                {
                    Id = x.Id,

                    SalesInvoiceId = x.SalesInvoiceId,

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
                .ToList()
        };
    }
}
using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private readonly TenantDbContext _context;

    public PurchaseInvoiceService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync()
    {
        return await _context.PurchaseInvoices
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.PurchaseInvoiceLines)
            .OrderByDescending(x => x.Id)
            .Select(x => new PurchaseInvoiceDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                PurchaseOrderId = x.PurchaseOrderId,
                GoodsReceiptId = x.GoodsReceiptId,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,
                SupplierInvoiceNumber = x.SupplierInvoiceNumber,
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
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Lines = x.PurchaseInvoiceLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new PurchaseInvoiceLineDto
                    {
                        Id = l.Id,
                        PurchaseInvoiceId = l.PurchaseInvoiceId,
                        GoodsReceiptLineId = l.GoodsReceiptLineId,
                        PurchaseOrderLineId = l.PurchaseOrderLineId,
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

    public async Task<PurchaseInvoiceDto?> GetByIdAsync(int id)
    {
        return await _context.PurchaseInvoices
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Include(x => x.PurchaseInvoiceLines)
            .Select(x => new PurchaseInvoiceDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                PurchaseOrderId = x.PurchaseOrderId,
                GoodsReceiptId = x.GoodsReceiptId,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,
                SupplierInvoiceNumber = x.SupplierInvoiceNumber,
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
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Lines = x.PurchaseInvoiceLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new PurchaseInvoiceLineDto
                    {
                        Id = l.Id,
                        PurchaseInvoiceId = l.PurchaseInvoiceId,
                        GoodsReceiptLineId = l.GoodsReceiptLineId,
                        PurchaseOrderLineId = l.PurchaseOrderLineId,
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
            .FirstOrDefaultAsync();
    }

    public async Task<PurchaseInvoiceDto> CreateAsync(
        PurchaseInvoiceDto dto)
    {
        var invoiceNumber =
            dto.InvoiceNumber.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new InvalidOperationException(
                "Invoice number is required.");

        if (dto.SupplierId <= 0)
            throw new InvalidOperationException(
                "SupplierId is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one invoice line is required.");

        if (dto.ExchangeRate <= 0)
            throw new InvalidOperationException(
                "ExchangeRate must be greater than zero.");

        var duplicate = await _context.PurchaseInvoices
            .AnyAsync(x =>
                x.InvoiceNumber == invoiceNumber &&
                x.IsActive);

        if (duplicate)
            throw new InvalidOperationException(
                $"Invoice number '{invoiceNumber}' already exists.");

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

        if (period == null)
            throw new InvalidOperationException(
                "Invalid or inactive FiscalYearPeriod.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new InvalidOperationException(
                "FiscalYearPeriod does not belong to FiscalYear.");

        if (dto.InvoiceDate.Date < period.StartDate.Date ||
            dto.InvoiceDate.Date > period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "InvoiceDate must be within fiscal period.");
        }

        if (dto.PurchaseOrderId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.PurchaseOrderId.Value &&
                    x.IsActive);

            if (po == null)
                throw new InvalidOperationException(
                    "Purchase Order not found or inactive.");

            if (po.SupplierId != dto.SupplierId)
                throw new InvalidOperationException(
                    "Supplier does not match Purchase Order.");
        }

        if (dto.GoodsReceiptId.HasValue)
        {
            var gr = await _context.GoodsReceipts
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.GoodsReceiptId.Value &&
                    x.IsActive);

            if (gr == null)
                throw new InvalidOperationException(
                    "Goods Receipt not found or inactive.");

            if (gr.SupplierId != dto.SupplierId)
                throw new InvalidOperationException(
                    "Supplier does not match Goods Receipt.");
        }

        var lineNumbers = dto.Lines
            .Select(x => x.LineNumber)
            .ToList();

        if (lineNumbers.Any(x => x <= 0))
            throw new InvalidOperationException(
                "LineNumber must be greater than zero.");

        if (lineNumbers.Distinct().Count() != lineNumbers.Count)
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var invoice = new PurchaseInvoice
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            SupplierId = dto.SupplierId,
            PurchaseOrderId = dto.PurchaseOrderId,
            GoodsReceiptId = dto.GoodsReceiptId,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = dto.InvoiceDate,
            SupplierInvoiceNumber = dto.SupplierInvoiceNumber,
            CurrencyId = dto.CurrencyId,
            ExchangeRate = dto.ExchangeRate,
            Status = "Draft",
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            BranchId = dto.BranchId,
            WarehouseId = dto.WarehouseId,
            IsPosted = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;
        decimal grandTotal = 0;

        foreach (var lineDto in dto.Lines)
        {
            if (lineDto.Quantity <= 0)
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");

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

            if (lineDto.GoodsReceiptLineId.HasValue)
            {
                var grLine = await _context.GoodsReceiptLines
                    .Include(x => x.GoodsReceipt)
                    .FirstOrDefaultAsync(x =>
                        x.Id == lineDto.GoodsReceiptLineId.Value &&
                        x.GoodsReceipt.IsActive);

                if (grLine == null)
                    throw new InvalidOperationException(
                        $"GoodsReceiptLine {lineDto.GoodsReceiptLineId} not found.");

                if (grLine.ProductId != lineDto.ProductId)
                    throw new InvalidOperationException(
                        "Product does not match GoodsReceiptLine.");

                if (lineDto.Quantity > grLine.ReceivedQuantity)
                    throw new InvalidOperationException(
                        "Invoice quantity cannot exceed received quantity.");
            }

            if (lineDto.PurchaseOrderLineId.HasValue)
            {
                var poLine = await _context.PurchaseOrderLines
                    .FirstOrDefaultAsync(x =>
                        x.Id == lineDto.PurchaseOrderLineId.Value);

                if (poLine == null)
                    throw new InvalidOperationException(
                        $"PurchaseOrderLine {lineDto.PurchaseOrderLineId} not found.");

                if (poLine.ProductId != lineDto.ProductId)
                    throw new InvalidOperationException(
                        "Product does not match PurchaseOrderLine.");
            }

            var gross =
                lineDto.Quantity *
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

            invoice.PurchaseInvoiceLines.Add(
                new PurchaseInvoiceLine
                {
                    GoodsReceiptLineId =
                        lineDto.GoodsReceiptLineId,

                    PurchaseOrderLineId =
                        lineDto.PurchaseOrderLineId,

                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,
                    Description = lineDto.Description,

                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,

                    DiscountPercent =
                        lineDto.DiscountPercent,

                    DiscountAmount = discount,

                    TaxPercent =
                        lineDto.TaxPercent,

                    TaxAmount = tax,

                    LineTotal = lineTotal,

                    LineNumber = lineDto.LineNumber
                });

            subtotal += gross;
            totalDiscount += discount;
            totalTax += tax;
            grandTotal += lineTotal;
        }

        invoice.SubTotal = subtotal;
        invoice.DiscountAmount = totalDiscount;
        invoice.TaxAmount = totalTax;
        invoice.GrandTotal = grandTotal;

        _context.PurchaseInvoices.Add(invoice);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(invoice.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseInvoiceDto dto)
    {
        var invoice = await _context.PurchaseInvoices
            .Include(x => x.PurchaseInvoiceLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (invoice == null)
            return false;

        if (invoice.IsPosted ||
            invoice.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft and unposted Purchase Invoice can be updated.");
        }

        dto.InvoiceNumber =
            dto.InvoiceNumber.Trim().ToUpperInvariant();

        var duplicate = await _context.PurchaseInvoices
            .AnyAsync(x =>
                x.Id != id &&
                x.InvoiceNumber == dto.InvoiceNumber &&
                x.IsActive);

        if (duplicate)
            throw new InvalidOperationException(
                $"Invoice number '{dto.InvoiceNumber}' already exists.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one invoice line is required.");

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

        if (dto.InvoiceDate.Date < period.StartDate.Date ||
            dto.InvoiceDate.Date > period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "InvoiceDate must be within fiscal period.");
        }

        if (dto.ExchangeRate <= 0)
            throw new InvalidOperationException(
                "ExchangeRate must be greater than zero.");

        _context.PurchaseInvoiceLines.RemoveRange(
            invoice.PurchaseInvoiceLines);

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
            if (lineDto.Quantity <= 0)
                throw new InvalidOperationException(
                    "Quantity must be greater than zero.");

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

            var gross =
                lineDto.Quantity *
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

            invoice.PurchaseInvoiceLines.Add(
                new PurchaseInvoiceLine
                {
                    PurchaseInvoiceId = id,
                    GoodsReceiptLineId =
                        lineDto.GoodsReceiptLineId,
                    PurchaseOrderLineId =
                        lineDto.PurchaseOrderLineId,
                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,
                    Description = lineDto.Description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    DiscountPercent =
                        lineDto.DiscountPercent,
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

        invoice.FiscalYearId = dto.FiscalYearId;
        invoice.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        invoice.SupplierId = dto.SupplierId;
        invoice.PurchaseOrderId = dto.PurchaseOrderId;
        invoice.GoodsReceiptId = dto.GoodsReceiptId;
        invoice.InvoiceNumber = dto.InvoiceNumber;
        invoice.InvoiceDate = dto.InvoiceDate;
        invoice.SupplierInvoiceNumber =
            dto.SupplierInvoiceNumber;
        invoice.CurrencyId = dto.CurrencyId;
        invoice.ExchangeRate = dto.ExchangeRate;
        invoice.ReferenceNumber = dto.ReferenceNumber;
        invoice.Notes = dto.Notes;
        invoice.BranchId = dto.BranchId;
        invoice.WarehouseId = dto.WarehouseId;

        invoice.SubTotal = subtotal;
        invoice.DiscountAmount = totalDiscount;
        invoice.TaxAmount = totalTax;
        invoice.GrandTotal = grandTotal;

        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (invoice == null)
            return false;

        if (invoice.IsPosted ||
            invoice.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft and unposted Purchase Invoice can be deleted.");
        }

        invoice.IsActive = false;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var invoice = await _context.PurchaseInvoices
                .Include(x => x.PurchaseInvoiceLines)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

            if (invoice == null)
                throw new InvalidOperationException(
                    "Purchase Invoice not found.");

            if (invoice.IsPosted ||
                invoice.Status == "Posted")
                throw new InvalidOperationException(
                    "Purchase Invoice is already posted.");

            if (invoice.Status != "Draft")
                throw new InvalidOperationException(
                    "Only Draft Purchase Invoice can be posted.");

            if (invoice.PurchaseInvoiceLines.Count == 0)
                throw new InvalidOperationException(
                    "Purchase Invoice must contain at least one line.");

            if (invoice.GrandTotal <= 0)
                throw new InvalidOperationException(
                    "Purchase Invoice total must be greater than zero.");

            /*
             * IMPORTANT:
             * Supplier SubLedger and Product PurchaseGLCode
             * are required for accounting posting.
             */

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(x =>
                    x.Id == invoice.SupplierId &&
                    x.IsActive);

            if (supplier == null)
                throw new InvalidOperationException(
                    "Supplier not found or inactive.");

            var supplierCode = supplier.Code?.Trim();

            if (string.IsNullOrWhiteSpace(supplierCode))
                throw new InvalidOperationException(
                    "Supplier Code is required for accounting posting.");

            var supplierSubLedger = await _context.SubLedgers
                .FirstOrDefaultAsync(x =>
                    x.Code == supplierCode &&
                    x.IsActive);

            if (supplierSubLedger == null)
                throw new InvalidOperationException(
                    $"SubLedger with supplier code '{supplierCode}' not found.");

            var payableAccountId =
                supplierSubLedger.LedgerAccountId;

            if (payableAccountId <= 0)
                throw new InvalidOperationException(
                    "Supplier SubLedger does not have a valid LedgerAccount.");

            decimal purchaseAmount = 0;

            var journal = new Journal
            {
                FiscalYearId = invoice.FiscalYearId,
                FiscalYearPeriodId = invoice.FiscalYearPeriodId,
                JournalNumber =
                    $"PINV-{invoice.InvoiceNumber}",
                JournalDate = invoice.InvoiceDate,
                ReferenceNumber =
                    invoice.SupplierInvoiceNumber
                    ?? invoice.InvoiceNumber,
                Description =
                    $"Purchase Invoice {invoice.InvoiceNumber}",
                JournalType = "PurchaseInvoice",
                IsPosted = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var line in invoice.PurchaseInvoiceLines)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == line.ProductId &&
                        x.IsActive);

                if (product == null)
                    throw new InvalidOperationException(
                        $"Product {line.ProductId} not found or inactive.");

                if (string.IsNullOrWhiteSpace(product.PurchaseGLCode))
                {
                    throw new InvalidOperationException(
                        $"Product {product.Id} does not have PurchaseGLCode.");
                }

                var purchaseAccount = await _context.LedgerAccounts
                    .FirstOrDefaultAsync(x =>
                        x.Code == product.PurchaseGLCode &&
                        x.IsActive);

                if (purchaseAccount == null)
                    throw new InvalidOperationException(
                        $"Purchase LedgerAccount '{product.PurchaseGLCode}' not found.");

                journal.JournalLines.Add(
                    new JournalLine
                    {
                        LedgerAccountId = purchaseAccount.Id,
                        Description =
                            line.Description
                            ?? $"Purchase - Product {line.ProductId}",
                        Debit = line.LineTotal,
                        Credit = 0,
                        LineNumber =
                            line.LineNumber * 2 - 1
                    });

                purchaseAmount += line.LineTotal;
            }

            /*
             * Supplier payable is credited with invoice total.
             */
            journal.JournalLines.Add(
                new JournalLine
                {
                    LedgerAccountId = payableAccountId,
                    SubLedgerId = supplierSubLedger.Id,
                    Description =
                        $"Accounts Payable - {supplier.Code}",
                    Debit = 0,
                    Credit = purchaseAmount,
                    LineNumber = 2
                });

            var totalDebit =
                journal.JournalLines.Sum(x => x.Debit);

            var totalCredit =
                journal.JournalLines.Sum(x => x.Credit);

            if (Math.Round(totalDebit, 8) !=
                Math.Round(totalCredit, 8))
            {
                throw new InvalidOperationException(
                    $"Journal is not balanced. Debit: {totalDebit}, Credit: {totalCredit}.");
            }

            _context.Journals.Add(journal);

            invoice.Status = "Posted";
            invoice.IsPosted = true;
            invoice.PostedAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            journal.IsPosted = true;
            journal.PostedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
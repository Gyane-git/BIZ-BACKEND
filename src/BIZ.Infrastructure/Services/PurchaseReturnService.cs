using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly TenantDbContext _context;

    public PurchaseReturnService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseReturnDto>> GetAllAsync()
    {
        return await _context.PurchaseReturns
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new PurchaseReturnDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                GoodsReceiptId = x.GoodsReceiptId,
                ReturnNumber = x.ReturnNumber,
                ReturnDate = x.ReturnDate,
                SupplierCreditNoteNumber =
                    x.SupplierCreditNoteNumber,
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

                Lines = x.PurchaseReturnLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new PurchaseReturnLineDto
                    {
                        Id = l.Id,
                        PurchaseReturnId =
                            l.PurchaseReturnId,
                        PurchaseInvoiceLineId =
                            l.PurchaseInvoiceLineId,
                        GoodsReceiptLineId =
                            l.GoodsReceiptLineId,
                        ProductId = l.ProductId,
                        UnitId = l.UnitId,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountPercent =
                            l.DiscountPercent,
                        DiscountAmount =
                            l.DiscountAmount,
                        TaxPercent = l.TaxPercent,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal,
                        LineNumber = l.LineNumber
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<PurchaseReturnDto?> GetByIdAsync(int id)
    {
        return await _context.PurchaseReturns
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.IsActive)
            .Select(x => new PurchaseReturnDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                GoodsReceiptId = x.GoodsReceiptId,
                ReturnNumber = x.ReturnNumber,
                ReturnDate = x.ReturnDate,
                SupplierCreditNoteNumber =
                    x.SupplierCreditNoteNumber,
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

                Lines = x.PurchaseReturnLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new PurchaseReturnLineDto
                    {
                        Id = l.Id,
                        PurchaseReturnId =
                            l.PurchaseReturnId,
                        PurchaseInvoiceLineId =
                            l.PurchaseInvoiceLineId,
                        GoodsReceiptLineId =
                            l.GoodsReceiptLineId,
                        ProductId = l.ProductId,
                        UnitId = l.UnitId,
                        Description = l.Description,
                        Quantity = l.Quantity,
                        UnitPrice = l.UnitPrice,
                        DiscountPercent =
                            l.DiscountPercent,
                        DiscountAmount =
                            l.DiscountAmount,
                        TaxPercent = l.TaxPercent,
                        TaxAmount = l.TaxAmount,
                        LineTotal = l.LineTotal,
                        LineNumber = l.LineNumber
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PurchaseReturnDto> CreateAsync(
        PurchaseReturnDto dto)
    {
        var returnNumber =
            dto.ReturnNumber.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(returnNumber))
            throw new InvalidOperationException(
                "Return number is required.");

        if (dto.SupplierId <= 0)
            throw new InvalidOperationException(
                "SupplierId is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one return line is required.");

        if (dto.ExchangeRate <= 0)
            throw new InvalidOperationException(
                "ExchangeRate must be greater than zero.");

        var duplicate = await _context.PurchaseReturns
            .AnyAsync(x =>
                x.ReturnNumber == returnNumber &&
                x.IsActive);

        if (duplicate)
            throw new InvalidOperationException(
                $"Return number '{returnNumber}' already exists.");

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

        if (dto.ReturnDate.Date < period.StartDate.Date ||
            dto.ReturnDate.Date > period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "ReturnDate must be within fiscal period.");
        }

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SupplierId &&
                x.IsActive);

        if (supplier == null)
            throw new InvalidOperationException(
                "Supplier not found or inactive.");

        if (dto.PurchaseInvoiceId.HasValue)
        {
            var invoice = await _context.PurchaseInvoices
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.PurchaseInvoiceId.Value &&
                    x.IsActive);

            if (invoice == null)
                throw new InvalidOperationException(
                    "Purchase Invoice not found or inactive.");

            if (invoice.SupplierId != dto.SupplierId)
                throw new InvalidOperationException(
                    "Supplier does not match Purchase Invoice.");
        }

        if (dto.GoodsReceiptId.HasValue)
        {
            var receipt = await _context.GoodsReceipts
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.GoodsReceiptId.Value &&
                    x.IsActive);

            if (receipt == null)
                throw new InvalidOperationException(
                    "Goods Receipt not found or inactive.");

            if (receipt.SupplierId != dto.SupplierId)
                throw new InvalidOperationException(
                    "Supplier does not match Goods Receipt.");
        }

        var lineNumbers = dto.Lines
            .Select(x => x.LineNumber)
            .ToList();

        if (lineNumbers.Any(x => x <= 0))
            throw new InvalidOperationException(
                "LineNumber must be greater than zero.");

        if (lineNumbers.Distinct().Count() !=
            lineNumbers.Count)
        {
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");
        }

        var entity = new PurchaseReturn
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            SupplierId = dto.SupplierId,
            PurchaseInvoiceId = dto.PurchaseInvoiceId,
            GoodsReceiptId = dto.GoodsReceiptId,
            ReturnNumber = returnNumber,
            ReturnDate = dto.ReturnDate,
            SupplierCreditNoteNumber =
                dto.SupplierCreditNoteNumber,
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
            ValidateLine(lineDto);

            var calculated =
                CalculateLine(lineDto);

            await ValidateSourceLinesAsync(
                lineDto,
                dto.PurchaseInvoiceId,
                dto.GoodsReceiptId);

            entity.PurchaseReturnLines.Add(
                new PurchaseReturnLine
                {
                    PurchaseInvoiceLineId =
                        lineDto.PurchaseInvoiceLineId,

                    GoodsReceiptLineId =
                        lineDto.GoodsReceiptLineId,

                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,
                    Description = lineDto.Description,

                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,

                    DiscountPercent =
                        lineDto.DiscountPercent,

                    DiscountAmount =
                        calculated.Discount,

                    TaxPercent =
                        lineDto.TaxPercent,

                    TaxAmount =
                        calculated.Tax,

                    LineTotal =
                        calculated.LineTotal,

                    LineNumber =
                        lineDto.LineNumber
                });

            subtotal += calculated.Gross;
            totalDiscount += calculated.Discount;
            totalTax += calculated.Tax;
            grandTotal += calculated.LineTotal;
        }

        entity.SubTotal = subtotal;
        entity.DiscountAmount = totalDiscount;
        entity.TaxAmount = totalTax;
        entity.GrandTotal = grandTotal;

        _context.PurchaseReturns.Add(entity);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchaseReturnDto dto)
    {
        var entity = await _context.PurchaseReturns
            .Include(x => x.PurchaseReturnLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted ||
            entity.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft and unposted Purchase Return can be updated.");
        }

        var returnNumber =
            dto.ReturnNumber.Trim().ToUpperInvariant();

        var duplicate = await _context.PurchaseReturns
            .AnyAsync(x =>
                x.Id != id &&
                x.ReturnNumber == returnNumber &&
                x.IsActive);

        if (duplicate)
            throw new InvalidOperationException(
                $"Return number '{returnNumber}' already exists.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one return line is required.");

        if (dto.ExchangeRate <= 0)
            throw new InvalidOperationException(
                "ExchangeRate must be greater than zero.");

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

        if (dto.ReturnDate.Date < period.StartDate.Date ||
            dto.ReturnDate.Date > period.EndDate.Date)
        {
            throw new InvalidOperationException(
                "ReturnDate must be within fiscal period.");
        }

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SupplierId &&
                x.IsActive);

        if (supplier == null)
            throw new InvalidOperationException(
                "Supplier not found or inactive.");

        _context.PurchaseReturnLines.RemoveRange(
            entity.PurchaseReturnLines);

        var lineNumbers = dto.Lines
            .Select(x => x.LineNumber)
            .ToList();

        if (lineNumbers.Any(x => x <= 0) ||
            lineNumbers.Distinct().Count() !=
            lineNumbers.Count)
        {
            throw new InvalidOperationException(
                "LineNumber must be unique and greater than zero.");
        }

        decimal subtotal = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;
        decimal grandTotal = 0;

        foreach (var lineDto in dto.Lines)
        {
            ValidateLine(lineDto);

            var calculated =
                CalculateLine(lineDto);

            await ValidateSourceLinesAsync(
                lineDto,
                dto.PurchaseInvoiceId,
                dto.GoodsReceiptId);

            entity.PurchaseReturnLines.Add(
                new PurchaseReturnLine
                {
                    PurchaseReturnId = id,

                    PurchaseInvoiceLineId =
                        lineDto.PurchaseInvoiceLineId,

                    GoodsReceiptLineId =
                        lineDto.GoodsReceiptLineId,

                    ProductId = lineDto.ProductId,
                    UnitId = lineDto.UnitId,
                    Description = lineDto.Description,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,

                    DiscountPercent =
                        lineDto.DiscountPercent,

                    DiscountAmount =
                        calculated.Discount,

                    TaxPercent =
                        lineDto.TaxPercent,

                    TaxAmount =
                        calculated.Tax,

                    LineTotal =
                        calculated.LineTotal,

                    LineNumber =
                        lineDto.LineNumber
                });

            subtotal += calculated.Gross;
            totalDiscount += calculated.Discount;
            totalTax += calculated.Tax;
            grandTotal += calculated.LineTotal;
        }

        entity.FiscalYearId = dto.FiscalYearId;
        entity.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        entity.SupplierId = dto.SupplierId;
        entity.PurchaseInvoiceId =
            dto.PurchaseInvoiceId;
        entity.GoodsReceiptId =
            dto.GoodsReceiptId;

        entity.ReturnNumber = returnNumber;
        entity.ReturnDate = dto.ReturnDate;

        entity.SupplierCreditNoteNumber =
            dto.SupplierCreditNoteNumber;

        entity.CurrencyId = dto.CurrencyId;
        entity.ExchangeRate = dto.ExchangeRate;

        entity.ReferenceNumber =
            dto.ReferenceNumber;

        entity.Notes = dto.Notes;
        entity.BranchId = dto.BranchId;
        entity.WarehouseId = dto.WarehouseId;

        entity.SubTotal = subtotal;
        entity.DiscountAmount = totalDiscount;
        entity.TaxAmount = totalTax;
        entity.GrandTotal = grandTotal;

        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.PurchaseReturns
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted ||
            entity.Status != "Draft")
        {
            throw new InvalidOperationException(
                "Only Draft and unposted Purchase Return can be deleted.");
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var entity = await _context.PurchaseReturns
                .Include(x => x.PurchaseReturnLines)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

            if (entity == null)
                throw new InvalidOperationException(
                    "Purchase Return not found.");

            if (entity.IsPosted ||
                entity.Status == "Posted")
            {
                throw new InvalidOperationException(
                    "Purchase Return is already posted.");
            }

            if (entity.Status != "Draft")
                throw new InvalidOperationException(
                    "Only Draft Purchase Return can be posted.");

            if (entity.PurchaseReturnLines.Count == 0)
                throw new InvalidOperationException(
                    "Purchase Return must contain at least one line.");

            if (entity.GrandTotal <= 0)
                throw new InvalidOperationException(
                    "Purchase Return total must be greater than zero.");

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(x =>
                    x.Id == entity.SupplierId &&
                    x.IsActive);

            if (supplier == null)
                throw new InvalidOperationException(
                    "Supplier not found or inactive.");

            var supplierCode =
                supplier.Code?.Trim();

            if (string.IsNullOrWhiteSpace(supplierCode))
                throw new InvalidOperationException(
                    "Supplier Code is required for accounting posting.");

            var supplierSubLedger =
                await _context.SubLedgers
                    .FirstOrDefaultAsync(x =>
                        x.Code == supplierCode &&
                        x.IsActive);

            if (supplierSubLedger == null)
            {
                throw new InvalidOperationException(
                    $"SubLedger with supplier code '{supplierCode}' not found.");
            }

            var payableAccountId =
                supplierSubLedger.LedgerAccountId;

            if (payableAccountId <= 0)
                throw new InvalidOperationException(
                    "Supplier SubLedger does not have a valid LedgerAccount.");

            var journal = new Journal
            {
                FiscalYearId =
                    entity.FiscalYearId,

                FiscalYearPeriodId =
                    entity.FiscalYearPeriodId,

                JournalNumber =
                    $"PRET-{entity.ReturnNumber}",

                JournalDate =
                    entity.ReturnDate,

                ReferenceNumber =
                    entity.SupplierCreditNoteNumber
                    ?? entity.ReturnNumber,

                Description =
                    $"Purchase Return {entity.ReturnNumber}",

                JournalType = "PurchaseReturn",

                IsPosted = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            decimal returnAmount = 0;

            foreach (var line in entity.PurchaseReturnLines)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == line.ProductId &&
                        x.IsActive);

                if (product == null)
                {
                    throw new InvalidOperationException(
                        $"Product {line.ProductId} not found or inactive.");
                }

                if (string.IsNullOrWhiteSpace(
                        product.PurchaseReturnGLCode))
                {
                    throw new InvalidOperationException(
                        $"Product {product.Id} does not have PurchaseReturnGLCode.");
                }

                var returnAccount =
                    await _context.LedgerAccounts
                        .FirstOrDefaultAsync(x =>
                            x.Code ==
                                product.PurchaseReturnGLCode &&
                            x.IsActive);

                if (returnAccount == null)
                {
                    throw new InvalidOperationException(
                        $"Purchase Return LedgerAccount '{product.PurchaseReturnGLCode}' not found.");
                }

                journal.JournalLines.Add(
                    new JournalLine
                    {
                        LedgerAccountId =
                            payableAccountId,

                        SubLedgerId =
                            supplierSubLedger.Id,

                        Description =
                            $"Accounts Payable - {supplier.Code}",

                        Debit =
                            line.LineTotal,

                        Credit = 0,

                        LineNumber =
                            line.LineNumber * 2 - 1
                    });

                journal.JournalLines.Add(
                    new JournalLine
                    {
                        LedgerAccountId =
                            returnAccount.Id,

                        Description =
                            line.Description
                            ?? $"Purchase Return - Product {line.ProductId}",

                        Debit = 0,

                        Credit =
                            line.LineTotal,

                        LineNumber =
                            line.LineNumber * 2
                    });

                returnAmount += line.LineTotal;
            }

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

            if (Math.Round(returnAmount, 8) !=
                Math.Round(entity.GrandTotal, 8))
            {
                throw new InvalidOperationException(
                    $"Return amount does not match invoice total. Return: {returnAmount}, Total: {entity.GrandTotal}.");
            }

            _context.Journals.Add(journal);

            entity.Status = "Posted";
            entity.IsPosted = true;
            entity.PostedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

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

    private static void ValidateLine(
        PurchaseReturnLineDto line)
    {
        if (line.ProductId <= 0)
            throw new InvalidOperationException(
                "ProductId is required.");

        if (line.Quantity <= 0)
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");

        if (line.UnitPrice < 0)
            throw new InvalidOperationException(
                "UnitPrice cannot be negative.");

        if (line.DiscountPercent < 0 ||
            line.DiscountPercent > 100)
        {
            throw new InvalidOperationException(
                "DiscountPercent must be between 0 and 100.");
        }

        if (line.TaxPercent < 0 ||
            line.TaxPercent > 100)
        {
            throw new InvalidOperationException(
                "TaxPercent must be between 0 and 100.");
        }
    }

    private static (
        decimal Gross,
        decimal Discount,
        decimal Tax,
        decimal LineTotal)
        CalculateLine(PurchaseReturnLineDto line)
    {
        var gross =
            line.Quantity * line.UnitPrice;

        var discount =
            gross * line.DiscountPercent / 100m;

        var taxable =
            gross - discount;

        var tax =
            taxable * line.TaxPercent / 100m;

        var total =
            taxable + tax;

        return (
            gross,
            discount,
            tax,
            total);
    }

    private async Task ValidateSourceLinesAsync(
        PurchaseReturnLineDto line,
        int? purchaseInvoiceId,
        int? goodsReceiptId)
    {
        if (line.PurchaseInvoiceLineId.HasValue)
        {
            var invoiceLine =
                await _context.PurchaseInvoiceLines
                    .Include(x => x.PurchaseInvoice)
                    .FirstOrDefaultAsync(x =>
                        x.Id ==
                        line.PurchaseInvoiceLineId.Value);

            if (invoiceLine == null)
            {
                throw new InvalidOperationException(
                    $"PurchaseInvoiceLine {line.PurchaseInvoiceLineId} not found.");
            }

            if (!invoiceLine.PurchaseInvoice.IsActive)
            {
                throw new InvalidOperationException(
                    "Source Purchase Invoice is inactive.");
            }

            if (purchaseInvoiceId.HasValue &&
                invoiceLine.PurchaseInvoiceId !=
                purchaseInvoiceId.Value)
            {
                throw new InvalidOperationException(
                    "PurchaseInvoiceLine does not belong to selected PurchaseInvoice.");
            }

            if (invoiceLine.ProductId != line.ProductId)
            {
                throw new InvalidOperationException(
                    "Product does not match PurchaseInvoiceLine.");
            }
        }

        if (line.GoodsReceiptLineId.HasValue)
        {
            var receiptLine =
                await _context.GoodsReceiptLines
                    .Include(x => x.GoodsReceipt)
                    .FirstOrDefaultAsync(x =>
                        x.Id ==
                        line.GoodsReceiptLineId.Value);

            if (receiptLine == null)
            {
                throw new InvalidOperationException(
                    $"GoodsReceiptLine {line.GoodsReceiptLineId} not found.");
            }

            if (!receiptLine.GoodsReceipt.IsActive)
            {
                throw new InvalidOperationException(
                    "Source Goods Receipt is inactive.");
            }

            if (goodsReceiptId.HasValue &&
                receiptLine.GoodsReceiptId !=
                goodsReceiptId.Value)
            {
                throw new InvalidOperationException(
                    "GoodsReceiptLine does not belong to selected GoodsReceipt.");
            }

            if (receiptLine.ProductId != line.ProductId)
            {
                throw new InvalidOperationException(
                    "Product does not match GoodsReceiptLine.");
            }

            if (line.Quantity > receiptLine.ReceivedQuantity)
            {
                throw new InvalidOperationException(
                    "Return quantity cannot exceed received quantity.");
            }
        }
    }
}
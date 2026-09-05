using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public sealed class SalesPostingService
{
    private readonly TenantDbContext _context;

    public SalesPostingService(TenantDbContext context)
    {
        _context = context;
    }

    public Task PostInvoiceAsync(SalesInvoice invoice) =>
        PostAsync(
            invoice.FiscalYearId,
            invoice.FiscalYearPeriodId,
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.CustomerId,
            invoice.GrandTotal,
            invoice.SalesInvoiceLines.Select(x => (x.ProductId, x.LineTotal)),
            false);

    public Task PostReturnAsync(SalesReturn salesReturn) =>
        PostAsync(
            salesReturn.FiscalYearId,
            salesReturn.FiscalYearPeriodId,
            salesReturn.ReturnNumber,
            salesReturn.ReturnDate,
            salesReturn.CustomerId,
            salesReturn.GrandTotal,
            salesReturn.SalesReturnLines.Select(x => (x.ProductId, x.LineTotal)),
            true);

    private async Task PostAsync(
        int fiscalYearId,
        int periodId,
        string documentNumber,
        DateTime documentDate,
        int customerId,
        decimal total,
        IEnumerable<(int ProductId, decimal Amount)> lines,
        bool isReturn)
    {
        if (total <= 0)
            throw new InvalidOperationException("Document total must be greater than zero.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x => x.Id == fiscalYearId && x.IsActive);
        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x => x.Id == periodId && x.FiscalYearId == fiscalYearId && x.IsActive);

        if (fiscalYear == null || fiscalYear.IsClosed)
            throw new InvalidOperationException("Fiscal year is not available for posting.");
        if (period == null || period.IsClosed)
            throw new InvalidOperationException("Fiscal year period is not available for posting.");
        if (documentDate.Date < period.StartDate.Date || documentDate.Date > period.EndDate.Date)
            throw new InvalidOperationException("Document date must be within the fiscal year period.");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == customerId && x.IsActive);
        if (customer == null)
            throw new InvalidOperationException("Customer not found.");

        var customerSubLedger = await _context.SubLedgers
            .FirstOrDefaultAsync(x => x.Code == customer.Code && x.IsActive);
        if (customerSubLedger == null)
            throw new InvalidOperationException(
                $"Active customer sub-ledger for customer code '{customer.Code}' is required before posting.");

        var productIds = lines.Select(x => x.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(x => productIds.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id);

        var accountAmounts = new Dictionary<int, decimal>();
        foreach (var line in lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
                throw new InvalidOperationException($"Product {line.ProductId} not found.");

            var glCode = isReturn ? product.SalesReturnGLCode : product.SalesGLCode;
            if (string.IsNullOrWhiteSpace(glCode))
                throw new InvalidOperationException(
                    $"{(isReturn ? "Sales return" : "Sales")} GL code is required for product '{product.Code}'.");

            var account = await _context.LedgerAccounts
                .FirstOrDefaultAsync(x => x.Code == glCode && x.IsActive);
            if (account == null)
                throw new InvalidOperationException($"Ledger account '{glCode}' was not found.");

            accountAmounts[account.Id] = accountAmounts.GetValueOrDefault(account.Id) + line.Amount;
        }

        var journalNumber = $"{(isReturn ? "SR" : "SI")}-{documentNumber.Trim().ToUpperInvariant()}";
        if (await _context.Journals.AnyAsync(x => x.JournalNumber == journalNumber && x.IsActive))
            throw new InvalidOperationException("This sales document has already been posted.");

        var journal = new Journal
        {
            FiscalYearId = fiscalYearId,
            FiscalYearPeriodId = periodId,
            JournalNumber = journalNumber,
            JournalDate = documentDate,
            ReferenceNumber = documentNumber,
            Description = isReturn ? $"Automatic journal for sales return {documentNumber}" :
                $"Automatic journal for sales invoice {documentNumber}",
            JournalType = isReturn ? "SalesReturn" : "Sales",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        journal.JournalLines.Add(new JournalLine
        {
            LedgerAccountId = customerSubLedger.LedgerAccountId,
            SubLedgerId = customerSubLedger.Id,
            Description = isReturn ? $"Customer credit for return {documentNumber}" :
                $"Customer receivable for invoice {documentNumber}",
            Debit = isReturn ? 0m : total,
            Credit = isReturn ? total : 0m,
            LineNumber = 1
        });

        var lineNumber = 2;
        foreach (var accountAmount in accountAmounts.OrderBy(x => x.Key))
        {
            journal.JournalLines.Add(new JournalLine
            {
                LedgerAccountId = accountAmount.Key,
                Description = isReturn ? $"Sales return {documentNumber}" : $"Sales {documentNumber}",
                Debit = isReturn ? accountAmount.Value : 0m,
                Credit = isReturn ? 0m : accountAmount.Value,
                LineNumber = lineNumber++
            });
        }

        var debit = journal.JournalLines.Sum(x => x.Debit);
        var credit = journal.JournalLines.Sum(x => x.Credit);
        if (Math.Abs(debit - credit) > 0.00000001m)
            throw new InvalidOperationException("Automatic sales journal is not balanced.");

        journal.IsPosted = true;
        journal.PostedAt = DateTime.UtcNow;
        _context.Journals.Add(journal);
        await _context.SaveChangesAsync();
    }
}

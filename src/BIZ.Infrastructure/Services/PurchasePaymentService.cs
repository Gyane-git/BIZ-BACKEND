using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PurchasePaymentService : IPurchasePaymentService
{
    private readonly TenantDbContext _context;

    public PurchasePaymentService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchasePaymentDto>> GetAllAsync()
    {
        return await _context.PurchasePayments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.PurchasePaymentAllocations
                .Where(a => a.IsActive))
            .OrderByDescending(x => x.Id)
            .Select(x => new PurchasePaymentDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                JournalId = x.JournalId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                PaymentNumber = x.PaymentNumber,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Allocations = x.PurchasePaymentAllocations
                    .Where(a => a.IsActive)
                    .Select(a => new PurchasePaymentAllocationDto
                    {
                        Id = a.Id,
                        PurchasePaymentId = a.PurchasePaymentId,
                        PurchaseInvoiceId = a.PurchaseInvoiceId,
                        AllocatedAmount = a.AllocatedAmount,
                        Notes = a.Notes,
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<PurchasePaymentDto?> GetByIdAsync(int id)
    {
        return await _context.PurchasePayments
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new PurchasePaymentDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                SupplierId = x.SupplierId,
                JournalId = x.JournalId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                PaymentNumber = x.PaymentNumber,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Allocations = x.PurchasePaymentAllocations
                    .Where(a => a.IsActive)
                    .Select(a => new PurchasePaymentAllocationDto
                    {
                        Id = a.Id,
                        PurchasePaymentId = a.PurchasePaymentId,
                        PurchaseInvoiceId = a.PurchaseInvoiceId,
                        AllocatedAmount = a.AllocatedAmount,
                        Notes = a.Notes,
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedAt,
                        UpdatedAt = a.UpdatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PurchasePaymentDto> CreateAsync(
        PurchasePaymentDto dto)
    {
        dto.PaymentNumber = dto.PaymentNumber.Trim().ToUpperInvariant();
        dto.PaymentMode = dto.PaymentMode.Trim();

        if (string.IsNullOrWhiteSpace(dto.PaymentNumber))
            throw new ArgumentException("PaymentNumber is required.");

        if (dto.SupplierId <= 0)
            throw new ArgumentException("SupplierId must be greater than zero.");

        if (dto.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.");

        if (dto.FiscalYearId <= 0)
            throw new ArgumentException("FiscalYearId is required.");

        if (dto.FiscalYearPeriodId <= 0)
            throw new ArgumentException("FiscalYearPeriodId is required.");

        if (dto.JournalId <= 0)
            throw new ArgumentException("JournalId is required.");

        if (dto.PaymentMode != "Cash" &&
            dto.PaymentMode != "Bank")
        {
            throw new ArgumentException(
                "PaymentMode must be either Cash or Bank.");
        }

        var duplicate = await _context.PurchasePayments
            .AnyAsync(x =>
                x.PaymentNumber == dto.PaymentNumber &&
                x.IsActive);

        if (duplicate)
            throw new ArgumentException(
                $"Payment number '{dto.PaymentNumber}' already exists.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new ArgumentException(
                "Fiscal year not found or inactive.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new ArgumentException(
                "Fiscal year period not found or inactive.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new ArgumentException(
                "FiscalYearPeriod does not belong to FiscalYear.");

        if (dto.PaymentDate < period.StartDate ||
            dto.PaymentDate > period.EndDate)
        {
            throw new ArgumentException(
                "PaymentDate must be within fiscal year period.");
        }

        if (dto.PaymentDate < fiscalYear.StartDate ||
            dto.PaymentDate > fiscalYear.EndDate)
        {
            throw new ArgumentException(
                "PaymentDate must be within fiscal year.");
        }

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x =>
                x.Id == dto.SupplierId &&
                x.IsActive);

        if (supplier == null)
            throw new ArgumentException(
                "Supplier not found or inactive.");

        var journal = await _context.Journals
            .FirstOrDefaultAsync(x =>
                x.Id == dto.JournalId &&
                x.IsActive);

        if (journal == null)
            throw new ArgumentException(
                "Journal not found or inactive.");

        if (journal.FiscalYearId != dto.FiscalYearId)
            throw new ArgumentException(
                "Journal FiscalYear does not match payment FiscalYear.");

        if (journal.FiscalYearPeriodId != dto.FiscalYearPeriodId)
            throw new ArgumentException(
                "Journal FiscalYearPeriod does not match payment period.");

        if (dto.PaymentMode == "Cash")
        {
            if (!dto.CashAccountId.HasValue)
                throw new ArgumentException(
                    "CashAccountId is required for Cash payment.");

            if (dto.BankAccountId.HasValue)
                throw new ArgumentException(
                    "BankAccountId must be null for Cash payment.");

            var cash = await _context.CashAccounts
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.CashAccountId.Value &&
                    x.IsActive);

            if (cash == null)
                throw new ArgumentException(
                    "Cash account not found or inactive.");
        }
        else
        {
            if (!dto.BankAccountId.HasValue)
                throw new ArgumentException(
                    "BankAccountId is required for Bank payment.");

            if (dto.CashAccountId.HasValue)
                throw new ArgumentException(
                    "CashAccountId must be null for Bank payment.");

            var bank = await _context.BankAccounts
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.BankAccountId.Value &&
                    x.IsActive);

            if (bank == null)
                throw new ArgumentException(
                    "Bank account not found or inactive.");
        }

        if (dto.Allocations == null)
            dto.Allocations = new List<PurchasePaymentAllocationDto>();

        if (dto.Allocations.Any(x => !x.IsActive))
        {
            throw new ArgumentException(
                "Inactive allocations cannot be created.");
        }

        var allocationTotal = dto.Allocations.Sum(
            x => x.AllocatedAmount);

        if (allocationTotal > dto.Amount)
        {
            throw new ArgumentException(
                "Total allocation cannot exceed payment amount.");
        }

        var invoiceIds = dto.Allocations
            .Select(x => x.PurchaseInvoiceId)
            .ToList();

        if (invoiceIds.Count != invoiceIds.Distinct().Count())
        {
            throw new ArgumentException(
                "The same PurchaseInvoice cannot be allocated twice.");
        }

        foreach (var allocation in dto.Allocations)
        {
            await ValidateAllocationAsync(
                allocation,
                dto.SupplierId);
        }

        var payment = new PurchasePayment
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            SupplierId = dto.SupplierId,
            JournalId = dto.JournalId,
            CashAccountId = dto.CashAccountId,
            BankAccountId = dto.BankAccountId,
            PaymentNumber = dto.PaymentNumber,
            PaymentDate = dto.PaymentDate,
            Amount = dto.Amount,
            PaymentMode = dto.PaymentMode,
            ReferenceNumber = dto.ReferenceNumber,
            Description = dto.Description,
            IsPosted = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var allocationDto in dto.Allocations)
        {
            payment.PurchasePaymentAllocations.Add(
                new PurchasePaymentAllocation
                {
                    PurchaseInvoiceId =
                        allocationDto.PurchaseInvoiceId,

                    AllocatedAmount =
                        allocationDto.AllocatedAmount,

                    Notes = allocationDto.Notes,

                    IsActive = true,

                    CreatedAt = DateTime.UtcNow
                });
        }

        _context.PurchasePayments.Add(payment);

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(payment.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        PurchasePaymentDto dto)
    {
        var payment = await _context.PurchasePayments
            .Include(x => x.PurchasePaymentAllocations)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (payment == null)
            return false;

        if (payment.IsPosted)
            throw new InvalidOperationException(
                "Posted payment cannot be updated.");

        dto.PaymentNumber =
            dto.PaymentNumber.Trim().ToUpperInvariant();

        dto.PaymentMode = dto.PaymentMode.Trim();

        if (dto.Amount <= 0)
            throw new ArgumentException(
                "Payment amount must be greater than zero.");

        if (dto.PaymentMode != "Cash" &&
            dto.PaymentMode != "Bank")
        {
            throw new ArgumentException(
                "PaymentMode must be either Cash or Bank.");
        }

        var duplicate = await _context.PurchasePayments
            .AnyAsync(x =>
                x.Id != id &&
                x.PaymentNumber == dto.PaymentNumber &&
                x.IsActive);

        if (duplicate)
            throw new ArgumentException(
                $"Payment number '{dto.PaymentNumber}' already exists.");

        if (dto.Allocations == null)
            dto.Allocations = new List<PurchasePaymentAllocationDto>();

        var allocationTotal = dto.Allocations
            .Sum(x => x.AllocatedAmount);

        if (allocationTotal > dto.Amount)
            throw new ArgumentException(
                "Total allocation cannot exceed payment amount.");

        var invoiceIds = dto.Allocations
            .Select(x => x.PurchaseInvoiceId)
            .ToList();

        if (invoiceIds.Count != invoiceIds.Distinct().Count())
            throw new ArgumentException(
                "The same PurchaseInvoice cannot be allocated twice.");

        foreach (var allocation in dto.Allocations)
        {
            await ValidateAllocationAsync(
                allocation,
                dto.SupplierId,
                payment.Id);
        }

        payment.FiscalYearId = dto.FiscalYearId;
        payment.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        payment.SupplierId = dto.SupplierId;
        payment.JournalId = dto.JournalId;
        payment.CashAccountId = dto.CashAccountId;
        payment.BankAccountId = dto.BankAccountId;
        payment.PaymentNumber = dto.PaymentNumber;
        payment.PaymentDate = dto.PaymentDate;
        payment.Amount = dto.Amount;
        payment.PaymentMode = dto.PaymentMode;
        payment.ReferenceNumber = dto.ReferenceNumber;
        payment.Description = dto.Description;
        payment.UpdatedAt = DateTime.UtcNow;

        foreach (var oldAllocation in payment.PurchasePaymentAllocations)
        {
            oldAllocation.IsActive = false;
            oldAllocation.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var allocationDto in dto.Allocations)
        {
            payment.PurchasePaymentAllocations.Add(
                new PurchasePaymentAllocation
                {
                    PurchaseInvoiceId =
                        allocationDto.PurchaseInvoiceId,

                    AllocatedAmount =
                        allocationDto.AllocatedAmount,

                    Notes = allocationDto.Notes,

                    IsActive = true,

                    CreatedAt = DateTime.UtcNow
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _context.PurchasePayments
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (payment == null)
            return false;

        if (payment.IsPosted)
            throw new InvalidOperationException(
                "Posted payment cannot be deleted.");

        payment.IsActive = false;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var payment = await _context.PurchasePayments
                .Include(x => x.PurchasePaymentAllocations
                    .Where(a => a.IsActive))
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsActive);

            if (payment == null)
                throw new ArgumentException(
                    "Purchase payment not found.");

            if (payment.IsPosted)
                throw new InvalidOperationException(
                    "Purchase payment is already posted.");

            if (payment.Amount <= 0)
                throw new ArgumentException(
                    "Payment amount must be greater than zero.");

            if (payment.PurchasePaymentAllocations
                .Sum(x => x.AllocatedAmount) > payment.Amount)
            {
                throw new InvalidOperationException(
                    "Allocation total cannot exceed payment amount.");
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(x =>
                    x.Id == payment.SupplierId &&
                    x.IsActive);

            if (supplier == null)
                throw new InvalidOperationException(
                    "Supplier not found or inactive.");

            if (string.IsNullOrWhiteSpace(supplier.Code))
                throw new InvalidOperationException(
                    "Supplier Code is required.");

            var subLedger = await _context.SubLedgers
                .FirstOrDefaultAsync(x =>
                    x.Code == supplier.Code &&
                    x.IsActive);

            if (subLedger == null)
                throw new InvalidOperationException(
                    $"SubLedger with code '{supplier.Code}' not found.");

            var journal = await _context.Journals
                .FirstOrDefaultAsync(x =>
                    x.Id == payment.JournalId &&
                    x.IsActive);

            if (journal == null)
                throw new InvalidOperationException(
                    "Payment journal not found.");

            if (journal.IsPosted)
                throw new InvalidOperationException(
                    "Payment journal is already posted.");

            if (journal.FiscalYearId != payment.FiscalYearId)
                throw new InvalidOperationException(
                    "Journal FiscalYear does not match payment.");

            if (journal.FiscalYearPeriodId !=
                payment.FiscalYearPeriodId)
                throw new InvalidOperationException(
                    "Journal FiscalYearPeriod does not match payment.");

            var payableLedger = await _context.LedgerAccounts
                .FirstOrDefaultAsync(x =>
                    x.Id == subLedger.LedgerAccountId &&
                    x.IsActive);

            if (payableLedger == null)
                throw new InvalidOperationException(
                    "Supplier payable LedgerAccount not found.");

            int cashBankLedgerId;

            if (payment.PaymentMode == "Cash")
            {
                if (!payment.CashAccountId.HasValue)
                    throw new InvalidOperationException(
                        "CashAccountId is required.");

                var cashAccount = await _context.CashAccounts
                    .FirstOrDefaultAsync(x =>
                        x.Id == payment.CashAccountId.Value &&
                        x.IsActive);

                if (cashAccount == null)
                    throw new InvalidOperationException(
                        "Cash account not found or inactive.");

                cashBankLedgerId = cashAccount.LedgerAccountId;
            }
            else if (payment.PaymentMode == "Bank")
            {
                if (!payment.BankAccountId.HasValue)
                    throw new InvalidOperationException(
                        "BankAccountId is required.");

                var bankAccount = await _context.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.Id == payment.BankAccountId.Value &&
                        x.IsActive);

                if (bankAccount == null)
                    throw new InvalidOperationException(
                        "Bank account not found or inactive.");

                cashBankLedgerId = bankAccount.LedgerAccountId;
            }
            else
            {
                throw new InvalidOperationException(
                    "PaymentMode must be Cash or Bank.");
            }

            var cashBankLedger = await _context.LedgerAccounts
                .FirstOrDefaultAsync(x =>
                    x.Id == cashBankLedgerId &&
                    x.IsActive);

            if (cashBankLedger == null)
                throw new InvalidOperationException(
                    "Cash/Bank LedgerAccount not found.");

            journal.JournalType = "PurchasePayment";
            journal.JournalDate = payment.PaymentDate;
            journal.ReferenceNumber =
                payment.ReferenceNumber ??
                payment.PaymentNumber;

            journal.Description =
                payment.Description ??
                $"Purchase payment {payment.PaymentNumber}";

            journal.IsPosted = false;

            var journalLines = await _context.JournalLines
                .Where(x => x.JournalId == journal.Id)
                .ToListAsync();

            if (journalLines.Any())
            {
                _context.JournalLines.RemoveRange(journalLines);
            }

            journal.JournalLines = new List<JournalLine>();

            journal.JournalLines.Add(
                new JournalLine
                {
                    LedgerAccountId =
                        payableLedger.Id,

                    SubLedgerId =
                        subLedger.Id,

                    Description =
                        $"Payment to supplier {supplier.Code}",

                    Debit = payment.Amount,

                    Credit = 0,

                    LineNumber = 1
                });

            journal.JournalLines.Add(
                new JournalLine
                {
                    LedgerAccountId =
                        cashBankLedger.Id,

                    Description =
                        $"{payment.PaymentMode} payment {payment.PaymentNumber}",

                    Debit = 0,

                    Credit = payment.Amount,

                    LineNumber = 2
                });

            var totalDebit =
                journal.JournalLines.Sum(x => x.Debit);

            var totalCredit =
                journal.JournalLines.Sum(x => x.Credit);

            if (totalDebit != totalCredit)
            {
                throw new InvalidOperationException(
                    "Purchase payment journal is not balanced.");
            }

            await _context.SaveChangesAsync();

            payment.IsPosted = true;
            payment.PostedAt = DateTime.UtcNow;

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

    private async Task ValidateAllocationAsync(
        PurchasePaymentAllocationDto allocation,
        int supplierId,
        int? currentPaymentId = null)
    {
        if (allocation.PurchaseInvoiceId <= 0)
            throw new ArgumentException(
                "PurchaseInvoiceId must be greater than zero.");

        if (allocation.AllocatedAmount <= 0)
            throw new ArgumentException(
                "AllocatedAmount must be greater than zero.");

        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x =>
                x.Id == allocation.PurchaseInvoiceId &&
                x.IsActive);

        if (invoice == null)
            throw new ArgumentException(
                "Purchase invoice not found or inactive.");

        if (invoice.SupplierId != supplierId)
            throw new ArgumentException(
                "Purchase invoice does not belong to the selected supplier.");

        if (currentPaymentId.HasValue)
        {
            var duplicate = await _context
                .PurchasePaymentAllocations
                .AnyAsync(x =>
                    x.PurchasePaymentId == currentPaymentId.Value &&
                    x.PurchaseInvoiceId ==
                        allocation.PurchaseInvoiceId &&
                    x.IsActive);

            if (duplicate)
                throw new ArgumentException(
                    "Invoice is already allocated to this payment.");
        }

        var allocatedAlready = await _context
            .PurchasePaymentAllocations
            .Where(x =>
                x.PurchaseInvoiceId ==
                    allocation.PurchaseInvoiceId &&
                x.IsActive &&
                (!currentPaymentId.HasValue ||
                 x.PurchasePaymentId !=
                    currentPaymentId.Value))
            .SumAsync(x => (decimal?)x.AllocatedAmount) ?? 0;

        var invoiceTotal = await _context.PurchaseInvoiceLines
            .Where(x =>
                x.PurchaseInvoiceId ==
                    allocation.PurchaseInvoiceId)
            .SumAsync(x => (decimal?)x.LineTotal) ?? 0;

        var remaining = invoiceTotal - allocatedAlready;

        if (allocation.AllocatedAmount > remaining)
        {
            throw new ArgumentException(
                $"Allocation {allocation.AllocatedAmount} exceeds invoice remaining balance {remaining}.");
        }
    }
}
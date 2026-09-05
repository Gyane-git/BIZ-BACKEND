using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class SalesPaymentService : ISalesPaymentService
{
    private readonly TenantDbContext _context;

    public SalesPaymentService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesPaymentDto>> GetAllAsync()
    {
        return await _context.SalesPayments
            .Include(x => x.SalesPaymentAllocations)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new SalesPaymentDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                CustomerId = x.CustomerId,
                PaymentNumber = x.PaymentNumber,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                JournalId = x.JournalId,
                Status = x.Status,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Allocations = x.SalesPaymentAllocations
                    .Select(a => new SalesPaymentAllocationDto
                    {
                        Id = a.Id,
                        SalesPaymentId = a.SalesPaymentId,
                        SalesInvoiceId = a.SalesInvoiceId,
                        AllocatedAmount = a.AllocatedAmount,
                        Notes = a.Notes,
                        CreatedAt = a.CreatedAt
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<SalesPaymentDto?> GetByIdAsync(int id)
    {
        return await _context.SalesPayments
            .Include(x => x.SalesPaymentAllocations)
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new SalesPaymentDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                CustomerId = x.CustomerId,
                PaymentNumber = x.PaymentNumber,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                JournalId = x.JournalId,
                Status = x.Status,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Allocations = x.SalesPaymentAllocations
                    .Select(a => new SalesPaymentAllocationDto
                    {
                        Id = a.Id,
                        SalesPaymentId = a.SalesPaymentId,
                        SalesInvoiceId = a.SalesInvoiceId,
                        AllocatedAmount = a.AllocatedAmount,
                        Notes = a.Notes,
                        CreatedAt = a.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SalesPaymentDto> CreateAsync(SalesPaymentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PaymentNumber))
            throw new Exception("Payment number is required.");

        if (dto.Amount <= 0)
            throw new Exception("Payment amount must be greater than zero.");

        if (dto.CustomerId <= 0)
            throw new Exception("CustomerId is required.");

        var duplicate = await _context.SalesPayments
            .AnyAsync(x => x.PaymentNumber == dto.PaymentNumber);

        if (duplicate)
            throw new Exception("Payment number already exists.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new Exception("Fiscal year not found.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new Exception("Fiscal year period not found.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new Exception(
                "Fiscal year period does not belong to fiscal year.");

        if (dto.PaymentDate.Date < period.StartDate.Date ||
            dto.PaymentDate.Date > period.EndDate.Date)
        {
            throw new Exception(
                "Payment date is outside fiscal year period.");
        }

        var mode = dto.PaymentMode.Trim();

        if (mode != "Cash" && mode != "Bank")
            throw new Exception(
                "PaymentMode must be either Cash or Bank.");

        if (mode == "Cash")
        {
            if (!dto.CashAccountId.HasValue)
                throw new Exception(
                    "CashAccountId is required for Cash payment.");

            if (dto.BankAccountId.HasValue)
                throw new Exception(
                    "BankAccountId must be empty for Cash payment.");

            var cashExists = await _context.CashAccounts
                .AnyAsync(x =>
                    x.Id == dto.CashAccountId.Value &&
                    x.IsActive);

            if (!cashExists)
                throw new Exception("Cash account not found.");
        }

        if (mode == "Bank")
        {
            if (!dto.BankAccountId.HasValue)
                throw new Exception(
                    "BankAccountId is required for Bank payment.");

            if (dto.CashAccountId.HasValue)
                throw new Exception(
                    "CashAccountId must be empty for Bank payment.");

            var bankExists = await _context.BankAccounts
                .AnyAsync(x =>
                    x.Id == dto.BankAccountId.Value &&
                    x.IsActive);

            if (!bankExists)
                throw new Exception("Bank account not found.");
        }

        if (dto.Allocations == null ||
            dto.Allocations.Count == 0)
        {
            throw new Exception(
                "At least one payment allocation is required.");
        }

        var invoiceIds = dto.Allocations
            .Select(x => x.SalesInvoiceId)
            .ToList();

        if (invoiceIds.Any(x => x <= 0))
            throw new Exception("Invalid SalesInvoiceId.");

        if (invoiceIds.Distinct().Count() != invoiceIds.Count)
            throw new Exception(
                "Duplicate invoice allocation is not allowed.");

        decimal allocationTotal = 0;

        foreach (var allocation in dto.Allocations)
        {
            if (allocation.AllocatedAmount <= 0)
                throw new Exception(
                    "Allocated amount must be greater than zero.");

            var invoice = await _context.SalesInvoices
                .FirstOrDefaultAsync(x =>
                    x.Id == allocation.SalesInvoiceId &&
                    x.IsActive);

            if (invoice == null)
                throw new Exception(
                    $"Sales invoice {allocation.SalesInvoiceId} not found.");

            if (invoice.CustomerId != dto.CustomerId)
                throw new Exception(
                    "Invoice customer does not match payment customer.");

            var availableBalance =
                invoice.GrandTotal - invoice.PaidAmount;

            if (allocation.AllocatedAmount > availableBalance)
            {
                throw new Exception(
                    $"Allocation exceeds invoice balance for invoice {invoice.Id}.");
            }

            allocationTotal += allocation.AllocatedAmount;
        }

        if (allocationTotal != dto.Amount)
        {
            throw new Exception(
                $"Payment amount ({dto.Amount}) must equal allocation total ({allocationTotal}).");
        }

        var payment = new SalesPayment
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            CustomerId = dto.CustomerId,
            PaymentNumber = dto.PaymentNumber.Trim(),
            PaymentDate = dto.PaymentDate,
            Amount = dto.Amount,
            PaymentMode = mode,
            ReferenceNumber = dto.ReferenceNumber,
            Description = dto.Description,
            CashAccountId = dto.CashAccountId,
            BankAccountId = dto.BankAccountId,
            JournalId = dto.JournalId,
            Status = "Draft",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var allocation in dto.Allocations)
        {
            payment.SalesPaymentAllocations.Add(
                new SalesPaymentAllocation
                {
                    SalesInvoiceId = allocation.SalesInvoiceId,
                    AllocatedAmount = allocation.AllocatedAmount,
                    Notes = allocation.Notes,
                    CreatedAt = DateTime.UtcNow
                });
        }

        _context.SalesPayments.Add(payment);

        await _context.SaveChangesAsync();

        foreach (var allocation in payment.SalesPaymentAllocations)
        {
            var invoice = await _context.SalesInvoices
                .FirstAsync(x =>
                    x.Id == allocation.SalesInvoiceId);

            invoice.PaidAmount += allocation.AllocatedAmount;

            invoice.BalanceAmount =
                invoice.GrandTotal - invoice.PaidAmount;

            if (invoice.BalanceAmount <= 0)
            {
                invoice.BalanceAmount = 0;
                invoice.Status = "Paid";
            }
            else
            {
                invoice.Status = "PartiallyPaid";
            }

            invoice.UpdatedAt = DateTime.UtcNow;
        }

        payment.Status = "Posted";

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(payment.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id,
        SalesPaymentDto dto)
    {
        var payment = await _context.SalesPayments
            .Include(x => x.SalesPaymentAllocations)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (payment == null)
            return false;

        if (payment.Status != "Draft")
            throw new Exception(
                "Only Draft payment can be updated.");

        throw new Exception(
            "SalesPayment update should be handled before posting.");
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var payment = await _context.SalesPayments
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (payment == null)
            return false;

        if (payment.Status != "Draft")
            throw new Exception(
                "Only Draft payment can be deleted.");

        payment.IsActive = false;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
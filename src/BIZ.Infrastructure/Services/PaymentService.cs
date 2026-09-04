using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly TenantDbContext _context;

    public PaymentService(
        TenantDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<List<PaymentDto>> GetAllAsync()
    {
        return await _context.Payments
            .AsNoTracking()
            .OrderByDescending(x => x.PaymentDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new PaymentDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                PaymentNumber = x.PaymentNumber,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<PaymentDto?> GetByIdAsync(
        int id)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PaymentDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                PaymentNumber = x.PaymentNumber,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // GET BY NUMBER
    // =========================================================

    public async Task<PaymentDto?> GetByNumberAsync(
        string paymentNumber)
    {
        paymentNumber =
            paymentNumber.Trim().ToUpper();

        return await _context.Payments
            .AsNoTracking()
            .Where(x =>
                x.PaymentNumber == paymentNumber)
            .Select(x => new PaymentDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                PaymentNumber = x.PaymentNumber,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // GET BY JOURNAL
    // =========================================================

    public async Task<List<PaymentDto>> GetByJournalAsync(
        int journalId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(x => x.JournalId == journalId)
            .Select(x => new PaymentDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                PaymentNumber = x.PaymentNumber,
                PaymentDate = x.PaymentDate,
                Amount = x.Amount,
                PaymentMode = x.PaymentMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task<PaymentDto> CreateAsync(
        PaymentDto dto)
    {
        dto.PaymentNumber =
            dto.PaymentNumber.Trim().ToUpper();

        dto.PaymentMode =
            string.IsNullOrWhiteSpace(dto.PaymentMode)
                ? "Cash"
                : dto.PaymentMode.Trim();

        if (dto.JournalId <= 0)
            throw new Exception(
                "Valid JournalId is required.");

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (string.IsNullOrWhiteSpace(
                dto.PaymentNumber))
            throw new Exception(
                "PaymentNumber is required.");

        if (dto.Amount <= 0)
            throw new Exception(
                "Payment Amount must be greater than zero.");

        // ---------------------------------------------------------
        // Payment Mode
        // ---------------------------------------------------------

        var allowedModes = new[]
        {
            "Cash",
            "Bank"
        };

        if (!allowedModes.Contains(
                dto.PaymentMode,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new Exception(
                "PaymentMode must be Cash or Bank.");
        }

        dto.PaymentMode =
            dto.PaymentMode.Equals(
                "Bank",
                StringComparison.OrdinalIgnoreCase)
                ? "Bank"
                : "Cash";

        // ---------------------------------------------------------
        // Journal
        // ---------------------------------------------------------

        var journal = await _context.Journals
            .FirstOrDefaultAsync(x =>
                x.Id == dto.JournalId &&
                x.IsActive);

        if (journal == null)
            throw new Exception(
                "Journal not found.");

        if (journal.IsPosted)
            throw new Exception(
                "Cannot create payment for a posted journal.");

        // ---------------------------------------------------------
        // Ledger Account
        // ---------------------------------------------------------

        var ledgerAccount =
            await _context.LedgerAccounts
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.LedgerAccountId &&
                    x.IsActive);

        if (ledgerAccount == null)
            throw new Exception(
                "Ledger Account not found.");

        // ---------------------------------------------------------
        // SubLedger
        // ---------------------------------------------------------

        if (dto.SubLedgerId.HasValue)
        {
            var subLedger =
                await _context.SubLedgers
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.SubLedgerId.Value &&
                        x.IsActive);

            if (subLedger == null)
                throw new Exception(
                    "SubLedger not found.");

            if (subLedger.LedgerAccountId !=
                dto.LedgerAccountId)
            {
                throw new Exception(
                    "SubLedger does not belong to selected Ledger Account.");
            }
        }

        // ---------------------------------------------------------
        // Payment Mode Account
        // ---------------------------------------------------------

        if (dto.PaymentMode == "Cash")
        {
            if (!dto.CashAccountId.HasValue)
                throw new Exception(
                    "CashAccountId is required for Cash payment.");

            if (dto.BankAccountId.HasValue)
                throw new Exception(
                    "BankAccountId must be null for Cash payment.");

            var cashAccount =
                await _context.CashAccounts
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.CashAccountId.Value &&
                        x.IsActive);

            if (cashAccount == null)
                throw new Exception(
                    "Cash Account not found.");
        }

        if (dto.PaymentMode == "Bank")
        {
            if (!dto.BankAccountId.HasValue)
                throw new Exception(
                    "BankAccountId is required for Bank payment.");

            if (dto.CashAccountId.HasValue)
                throw new Exception(
                    "CashAccountId must be null for Bank payment.");

            var bankAccount =
                await _context.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.BankAccountId.Value &&
                        x.IsActive);

            if (bankAccount == null)
                throw new Exception(
                    "Bank Account not found.");
        }

        // ---------------------------------------------------------
        // Payment Number
        // ---------------------------------------------------------

        var numberExists =
            await _context.Payments
                .AnyAsync(x =>
                    x.PaymentNumber ==
                    dto.PaymentNumber);

        if (numberExists)
            throw new Exception(
                "Payment Number already exists.");

        // ---------------------------------------------------------
        // Create
        // ---------------------------------------------------------

        var entity = new Payment
        {
            JournalId = dto.JournalId,
            LedgerAccountId = dto.LedgerAccountId,
            SubLedgerId = dto.SubLedgerId,
            CashAccountId = dto.CashAccountId,
            BankAccountId = dto.BankAccountId,
            PaymentNumber = dto.PaymentNumber,
            PaymentDate = dto.PaymentDate,
            Amount = dto.Amount,
            PaymentMode = dto.PaymentMode,
            ReferenceNumber =
                dto.ReferenceNumber?.Trim(),
            Description =
                dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        PaymentDto dto)
    {
        var entity =
            await _context.Payments
                .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var journal =
            await _context.Journals
                .FirstOrDefaultAsync(x =>
                    x.Id == entity.JournalId &&
                    x.IsActive);

        if (journal == null)
            throw new Exception(
                "Journal not found.");

        if (journal.IsPosted)
            throw new Exception(
                "Cannot modify payment of a posted journal.");

        dto.PaymentNumber =
            dto.PaymentNumber.Trim().ToUpper();

        dto.PaymentMode =
            string.IsNullOrWhiteSpace(dto.PaymentMode)
                ? "Cash"
                : dto.PaymentMode.Trim();

        if (dto.JournalId != entity.JournalId)
            throw new Exception(
                "JournalId cannot be changed.");

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (dto.Amount <= 0)
            throw new Exception(
                "Payment Amount must be greater than zero.");

        if (dto.PaymentMode != "Cash" &&
            dto.PaymentMode != "Bank")
        {
            throw new Exception(
                "PaymentMode must be Cash or Bank.");
        }

        var ledgerAccount =
            await _context.LedgerAccounts
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.LedgerAccountId &&
                    x.IsActive);

        if (ledgerAccount == null)
            throw new Exception(
                "Ledger Account not found.");

        if (dto.SubLedgerId.HasValue)
        {
            var subLedger =
                await _context.SubLedgers
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.SubLedgerId.Value &&
                        x.IsActive);

            if (subLedger == null)
                throw new Exception(
                    "SubLedger not found.");

            if (subLedger.LedgerAccountId !=
                dto.LedgerAccountId)
            {
                throw new Exception(
                    "SubLedger does not belong to selected Ledger Account.");
            }
        }

        if (dto.PaymentMode == "Cash")
        {
            if (!dto.CashAccountId.HasValue)
                throw new Exception(
                    "CashAccountId is required for Cash payment.");

            dto.BankAccountId = null;

            var cashAccount =
                await _context.CashAccounts
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.CashAccountId.Value &&
                        x.IsActive);

            if (cashAccount == null)
                throw new Exception(
                    "Cash Account not found.");
        }
        else
        {
            if (!dto.BankAccountId.HasValue)
                throw new Exception(
                    "BankAccountId is required for Bank payment.");

            dto.CashAccountId = null;

            var bankAccount =
                await _context.BankAccounts
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.BankAccountId.Value &&
                        x.IsActive);

            if (bankAccount == null)
                throw new Exception(
                    "Bank Account not found.");
        }

        var numberExists =
            await _context.Payments
                .AnyAsync(x =>
                    x.PaymentNumber ==
                    dto.PaymentNumber &&
                    x.Id != id);

        if (numberExists)
            throw new Exception(
                "Payment Number already exists.");

        entity.LedgerAccountId =
            dto.LedgerAccountId;

        entity.SubLedgerId =
            dto.SubLedgerId;

        entity.CashAccountId =
            dto.CashAccountId;

        entity.BankAccountId =
            dto.BankAccountId;

        entity.PaymentNumber =
            dto.PaymentNumber;

        entity.PaymentDate =
            dto.PaymentDate;

        entity.Amount =
            dto.Amount;

        entity.PaymentMode =
            dto.PaymentMode;

        entity.ReferenceNumber =
            dto.ReferenceNumber?.Trim();

        entity.Description =
            dto.Description?.Trim();

        entity.IsActive =
            dto.IsActive;

        entity.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(
        int id)
    {
        var entity =
            await _context.Payments
                .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var journal =
            await _context.Journals
                .FirstOrDefaultAsync(x =>
                    x.Id == entity.JournalId &&
                    x.IsActive);

        if (journal?.IsPosted == true)
            throw new Exception(
                "Cannot delete payment of a posted journal.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
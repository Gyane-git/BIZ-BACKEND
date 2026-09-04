using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ReceiptService : IReceiptService
{
    private readonly TenantDbContext _context;

    public ReceiptService(
        TenantDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<List<ReceiptDto>> GetAllAsync()
    {
        return await _context.Receipts
            .AsNoTracking()
            .OrderByDescending(x => x.ReceiptDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new ReceiptDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                ReceiptNumber = x.ReceiptNumber,
                ReceiptDate = x.ReceiptDate,
                Amount = x.Amount,
                ReceiptMode = x.ReceiptMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<ReceiptDto?> GetByIdAsync(
        int id)
    {
        return await _context.Receipts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ReceiptDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                ReceiptNumber = x.ReceiptNumber,
                ReceiptDate = x.ReceiptDate,
                Amount = x.Amount,
                ReceiptMode = x.ReceiptMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // GET BY NUMBER
    // =========================================================

    public async Task<ReceiptDto?> GetByNumberAsync(
        string receiptNumber)
    {
        receiptNumber =
            receiptNumber.Trim().ToUpper();

        return await _context.Receipts
            .AsNoTracking()
            .Where(x =>
                x.ReceiptNumber ==
                receiptNumber)
            .Select(x => new ReceiptDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                ReceiptNumber = x.ReceiptNumber,
                ReceiptDate = x.ReceiptDate,
                Amount = x.Amount,
                ReceiptMode = x.ReceiptMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // GET BY JOURNAL
    // =========================================================

    public async Task<List<ReceiptDto>> GetByJournalAsync(
        int journalId)
    {
        return await _context.Receipts
            .AsNoTracking()
            .Where(x => x.JournalId == journalId)
            .Select(x => new ReceiptDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CashAccountId = x.CashAccountId,
                BankAccountId = x.BankAccountId,
                ReceiptNumber = x.ReceiptNumber,
                ReceiptDate = x.ReceiptDate,
                Amount = x.Amount,
                ReceiptMode = x.ReceiptMode,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task<ReceiptDto> CreateAsync(
        ReceiptDto dto)
    {
        dto.ReceiptNumber =
            dto.ReceiptNumber.Trim().ToUpper();

        dto.ReceiptMode =
            string.IsNullOrWhiteSpace(dto.ReceiptMode)
                ? "Cash"
                : dto.ReceiptMode.Trim();

        if (dto.JournalId <= 0)
            throw new Exception(
                "Valid JournalId is required.");

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (string.IsNullOrWhiteSpace(
                dto.ReceiptNumber))
            throw new Exception(
                "ReceiptNumber is required.");

        if (dto.Amount <= 0)
            throw new Exception(
                "Receipt Amount must be greater than zero.");

        // ---------------------------------------------------------
        // Receipt Mode
        // ---------------------------------------------------------

        var allowedModes = new[]
        {
            "Cash",
            "Bank"
        };

        if (!allowedModes.Contains(
                dto.ReceiptMode,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new Exception(
                "ReceiptMode must be Cash or Bank.");
        }

        dto.ReceiptMode =
            dto.ReceiptMode.Equals(
                "Bank",
                StringComparison.OrdinalIgnoreCase)
                ? "Bank"
                : "Cash";

        // ---------------------------------------------------------
        // Journal
        // ---------------------------------------------------------

        var journal =
            await _context.Journals
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.JournalId &&
                    x.IsActive);

        if (journal == null)
            throw new Exception(
                "Journal not found.");

        if (journal.IsPosted)
            throw new Exception(
                "Cannot create receipt for a posted journal.");

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
        // Cash / Bank
        // ---------------------------------------------------------

        if (dto.ReceiptMode == "Cash")
        {
            if (!dto.CashAccountId.HasValue)
                throw new Exception(
                    "CashAccountId is required for Cash receipt.");

            if (dto.BankAccountId.HasValue)
                throw new Exception(
                    "BankAccountId must be null for Cash receipt.");

            var cashAccount =
                await _context.CashAccounts
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.CashAccountId.Value &&
                        x.IsActive);

            if (cashAccount == null)
                throw new Exception(
                    "Cash Account not found.");
        }

        if (dto.ReceiptMode == "Bank")
        {
            if (!dto.BankAccountId.HasValue)
                throw new Exception(
                    "BankAccountId is required for Bank receipt.");

            if (dto.CashAccountId.HasValue)
                throw new Exception(
                    "CashAccountId must be null for Bank receipt.");

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
        // Receipt Number
        // ---------------------------------------------------------

        var numberExists =
            await _context.Receipts
                .AnyAsync(x =>
                    x.ReceiptNumber ==
                    dto.ReceiptNumber);

        if (numberExists)
            throw new Exception(
                "Receipt Number already exists.");

        // ---------------------------------------------------------
        // Create
        // ---------------------------------------------------------

        var entity = new Receipt
        {
            JournalId = dto.JournalId,
            LedgerAccountId = dto.LedgerAccountId,
            SubLedgerId = dto.SubLedgerId,
            CashAccountId = dto.CashAccountId,
            BankAccountId = dto.BankAccountId,
            ReceiptNumber = dto.ReceiptNumber,
            ReceiptDate = dto.ReceiptDate,
            Amount = dto.Amount,
            ReceiptMode = dto.ReceiptMode,
            ReferenceNumber =
                dto.ReferenceNumber?.Trim(),
            Description =
                dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Receipts.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        ReceiptDto dto)
    {
        var entity =
            await _context.Receipts
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
                "Cannot modify receipt of a posted journal.");

        dto.ReceiptNumber =
            dto.ReceiptNumber.Trim().ToUpper();

        dto.ReceiptMode =
            string.IsNullOrWhiteSpace(dto.ReceiptMode)
                ? "Cash"
                : dto.ReceiptMode.Trim();

        if (dto.JournalId != entity.JournalId)
            throw new Exception(
                "JournalId cannot be changed.");

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (dto.Amount <= 0)
            throw new Exception(
                "Receipt Amount must be greater than zero.");

        if (dto.ReceiptMode != "Cash" &&
            dto.ReceiptMode != "Bank")
        {
            throw new Exception(
                "ReceiptMode must be Cash or Bank.");
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

        if (dto.ReceiptMode == "Cash")
        {
            if (!dto.CashAccountId.HasValue)
                throw new Exception(
                    "CashAccountId is required for Cash receipt.");

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
                    "BankAccountId is required for Bank receipt.");

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
            await _context.Receipts
                .AnyAsync(x =>
                    x.ReceiptNumber ==
                    dto.ReceiptNumber &&
                    x.Id != id);

        if (numberExists)
            throw new Exception(
                "Receipt Number already exists.");

        entity.LedgerAccountId =
            dto.LedgerAccountId;

        entity.SubLedgerId =
            dto.SubLedgerId;

        entity.CashAccountId =
            dto.CashAccountId;

        entity.BankAccountId =
            dto.BankAccountId;

        entity.ReceiptNumber =
            dto.ReceiptNumber;

        entity.ReceiptDate =
            dto.ReceiptDate;

        entity.Amount =
            dto.Amount;

        entity.ReceiptMode =
            dto.ReceiptMode;

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
            await _context.Receipts
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
                "Cannot delete receipt of a posted journal.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
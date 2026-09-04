using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class JournalLineService : IJournalLineService
{
    private readonly TenantDbContext _context;

    public JournalLineService(
        TenantDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET ALL
    // =========================================================

    public async Task<List<JournalLineDto>> GetAllAsync()
    {
        return await _context.JournalLines
            .AsNoTracking()
            .OrderBy(x => x.JournalId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new JournalLineDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CostCenterId = x.CostCenterId,
                Description = x.Description,
                Debit = x.Debit,
                Credit = x.Credit,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }


    // =========================================================
    // GET BY JOURNAL
    // =========================================================

    public async Task<List<JournalLineDto>> GetByJournalAsync(
        int journalId)
    {
        return await _context.JournalLines
            .AsNoTracking()
            .Where(x => x.JournalId == journalId)
            .OrderBy(x => x.LineNumber)
            .Select(x => new JournalLineDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CostCenterId = x.CostCenterId,
                Description = x.Description,
                Debit = x.Debit,
                Credit = x.Credit,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<JournalLineDto?> GetByIdAsync(
        int id)
    {
        return await _context.JournalLines
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new JournalLineDto
            {
                Id = x.Id,
                JournalId = x.JournalId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CostCenterId = x.CostCenterId,
                Description = x.Description,
                Debit = x.Debit,
                Credit = x.Credit,
                LineNumber = x.LineNumber
            })
            .FirstOrDefaultAsync();
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task<JournalLineDto> CreateAsync(
        JournalLineDto dto)
    {
        if (dto.JournalId <= 0)
            throw new Exception(
                "Valid JournalId is required.");

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (dto.LineNumber <= 0)
            throw new Exception(
                "LineNumber must be greater than zero.");

        if (dto.Debit < 0 || dto.Credit < 0)
            throw new Exception(
                "Debit and Credit cannot be negative.");

        if (dto.Debit == 0 && dto.Credit == 0)
            throw new Exception(
                "Either Debit or Credit must be greater than zero.");

        if (dto.Debit > 0 && dto.Credit > 0)
            throw new Exception(
                "A journal line cannot have both Debit and Credit.");

        // ---------------------------------------------------------
        // Journal validation
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
                "Cannot modify a posted journal.");

        // ---------------------------------------------------------
        // Ledger Account validation
        // ---------------------------------------------------------

        var ledgerAccount = await _context.LedgerAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.LedgerAccountId &&
                x.IsActive);

        if (ledgerAccount == null)
            throw new Exception(
                "Ledger Account not found.");

        // ---------------------------------------------------------
        // SubLedger validation
        // ---------------------------------------------------------

        if (dto.SubLedgerId.HasValue)
        {
            var subLedger = await _context.SubLedgers
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
                    "SubLedger does not belong to the selected Ledger Account.");
            }
        }

        // ---------------------------------------------------------
        // CostCenter validation
        // ---------------------------------------------------------

        if (dto.CostCenterId.HasValue)
        {
            var costCenter = await _context.CostCenters
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.CostCenterId.Value &&
                    x.IsActive);

            if (costCenter == null)
                throw new Exception(
                    "Cost Center not found.");
        }

        // ---------------------------------------------------------
        // Line Number duplicate validation
        // ---------------------------------------------------------

        var lineExists = await _context.JournalLines
            .AnyAsync(x =>
                x.JournalId == dto.JournalId &&
                x.LineNumber == dto.LineNumber);

        if (lineExists)
            throw new Exception(
                "LineNumber already exists in this journal.");

        // ---------------------------------------------------------
        // Create
        // ---------------------------------------------------------

        var entity = new JournalLine
        {
            JournalId = dto.JournalId,
            LedgerAccountId = dto.LedgerAccountId,
            SubLedgerId = dto.SubLedgerId,
            CostCenterId = dto.CostCenterId,
            Description = dto.Description?.Trim(),
            Debit = dto.Debit,
            Credit = dto.Credit,
            LineNumber = dto.LineNumber
        };

        _context.JournalLines.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.Description = entity.Description;

        return dto;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        JournalLineDto dto)
    {
        var entity = await _context.JournalLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        // ---------------------------------------------------------
        // Journal validation
        // ---------------------------------------------------------

        var journal = await _context.Journals
            .FirstOrDefaultAsync(x =>
                x.Id == entity.JournalId &&
                x.IsActive);

        if (journal == null)
            throw new Exception(
                "Journal not found.");

        if (journal.IsPosted)
            throw new Exception(
                "Cannot modify a posted journal.");

        // JournalId cannot be changed
        if (dto.JournalId != entity.JournalId)
            throw new Exception(
                "JournalId cannot be changed.");

        if (dto.LedgerAccountId <= 0)
            throw new Exception(
                "Valid LedgerAccountId is required.");

        if (dto.LineNumber <= 0)
            throw new Exception(
                "LineNumber must be greater than zero.");

        if (dto.Debit < 0 || dto.Credit < 0)
            throw new Exception(
                "Debit and Credit cannot be negative.");

        if (dto.Debit == 0 && dto.Credit == 0)
            throw new Exception(
                "Either Debit or Credit must be greater than zero.");

        if (dto.Debit > 0 && dto.Credit > 0)
            throw new Exception(
                "A journal line cannot have both Debit and Credit.");

        // ---------------------------------------------------------
        // Ledger Account
        // ---------------------------------------------------------

        var ledgerAccount = await _context.LedgerAccounts
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
            var subLedger = await _context.SubLedgers
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
                    "SubLedger does not belong to the selected Ledger Account.");
            }
        }

        // ---------------------------------------------------------
        // CostCenter
        // ---------------------------------------------------------

        if (dto.CostCenterId.HasValue)
        {
            var costCenter = await _context.CostCenters
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.CostCenterId.Value &&
                    x.IsActive);

            if (costCenter == null)
                throw new Exception(
                    "Cost Center not found.");
        }

        // ---------------------------------------------------------
        // Duplicate line number
        // ---------------------------------------------------------

        var lineExists = await _context.JournalLines
            .AnyAsync(x =>
                x.JournalId == entity.JournalId &&
                x.LineNumber == dto.LineNumber &&
                x.Id != id);

        if (lineExists)
            throw new Exception(
                "LineNumber already exists in this journal.");

        // ---------------------------------------------------------
        // Update
        // ---------------------------------------------------------

        entity.LedgerAccountId = dto.LedgerAccountId;
        entity.SubLedgerId = dto.SubLedgerId;
        entity.CostCenterId = dto.CostCenterId;
        entity.Description = dto.Description?.Trim();
        entity.Debit = dto.Debit;
        entity.Credit = dto.Credit;
        entity.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================================================
    // DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(
        int id)
    {
        var entity = await _context.JournalLines
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var journal = await _context.Journals
            .FirstOrDefaultAsync(x =>
                x.Id == entity.JournalId &&
                x.IsActive);

        if (journal == null)
            throw new Exception(
                "Journal not found.");

        if (journal.IsPosted)
            throw new Exception(
                "Cannot delete a line from a posted journal.");

        _context.JournalLines.Remove(entity);

        await _context.SaveChangesAsync();

        return true;
    }
}
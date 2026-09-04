using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;
public class DebitNoteService : IDebitNoteService
{
    private readonly TenantDbContext _context;

    public DebitNoteService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<DebitNoteDto>> GetAllAsync()
    {
        return await _context.DebitNotes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new DebitNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                DebitNoteNumber = x.DebitNoteNumber,
                DebitNoteDate = x.DebitNoteDate,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                TotalAmount = x.TotalAmount,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<DebitNoteDto?> GetByIdAsync(int id)
    {
        return await _context.DebitNotes
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new DebitNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                DebitNoteNumber = x.DebitNoteNumber,
                DebitNoteDate = x.DebitNoteDate,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                TotalAmount = x.TotalAmount,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<DebitNoteDto?> GetByNumberAsync(string debitNoteNumber)
    {
        debitNoteNumber = debitNoteNumber.Trim().ToUpper();

        return await _context.DebitNotes
            .AsNoTracking()
            .Where(x =>
                x.DebitNoteNumber == debitNoteNumber &&
                x.IsActive)
            .Select(x => new DebitNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                DebitNoteNumber = x.DebitNoteNumber,
                DebitNoteDate = x.DebitNoteDate,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                TotalAmount = x.TotalAmount,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<DebitNoteDto>> GetByFiscalYearAsync(int fiscalYearId)
    {
        return await _context.DebitNotes
            .AsNoTracking()
            .Where(x =>
                x.FiscalYearId == fiscalYearId &&
                x.IsActive)
            .OrderByDescending(x => x.DebitNoteDate)
            .Select(x => new DebitNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                DebitNoteNumber = x.DebitNoteNumber,
                DebitNoteDate = x.DebitNoteDate,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                TotalAmount = x.TotalAmount,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<DebitNoteDto>> GetByPeriodAsync(int fiscalYearPeriodId)
    {
        return await _context.DebitNotes
            .AsNoTracking()
            .Where(x =>
                x.FiscalYearPeriodId == fiscalYearPeriodId &&
                x.IsActive)
            .OrderByDescending(x => x.DebitNoteDate)
            .Select(x => new DebitNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                DebitNoteNumber = x.DebitNoteNumber,
                DebitNoteDate = x.DebitNoteDate,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                TotalAmount = x.TotalAmount,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<DebitNoteDto> CreateAsync(DebitNoteDto dto)
    {
        var number = dto.DebitNoteNumber.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(number))
            throw new Exception("Debit note number is required.");

        if (dto.DebitNoteDate == default)
            throw new Exception("Debit note date is required.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new Exception("Fiscal year not found.");

        if (fiscalYear.IsClosed)
            throw new Exception("Fiscal year is closed.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new Exception("Fiscal year period not found.");

        if (period.IsClosed)
            throw new Exception("Fiscal year period is closed.");

        if (period.FiscalYearId != dto.FiscalYearId)
            throw new Exception(
                "Fiscal year period does not belong to selected fiscal year.");

        if (dto.DebitNoteDate < period.StartDate ||
            dto.DebitNoteDate > period.EndDate)
            throw new Exception(
                "Debit note date must be within fiscal year period.");

        var ledger = await _context.LedgerAccounts
            .FirstOrDefaultAsync(x =>
                x.Id == dto.LedgerAccountId &&
                x.IsActive);

        if (ledger == null)
            throw new Exception("Ledger account not found.");

        if (dto.SubLedgerId.HasValue)
        {
            var subLedger = await _context.SubLedgers
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.SubLedgerId.Value &&
                    x.IsActive);

            if (subLedger == null)
                throw new Exception("Sub ledger not found.");

            if (subLedger.LedgerAccountId != dto.LedgerAccountId)
                throw new Exception(
                    "Sub ledger does not belong to selected ledger account.");
        }

        var exists = await _context.DebitNotes
            .AnyAsync(x =>
                x.DebitNoteNumber == number &&
                x.IsActive);

        if (exists)
            throw new Exception("Debit note number already exists.");

        var entity = new DebitNote
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            LedgerAccountId = dto.LedgerAccountId,
            SubLedgerId = dto.SubLedgerId,

            DebitNoteNumber = number,
            DebitNoteDate = dto.DebitNoteDate,

            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Reason = dto.Reason?.Trim(),

            TotalAmount = 0,

            IsPosted = false,
            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        _context.DebitNotes.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.DebitNoteNumber = entity.DebitNoteNumber;
        dto.TotalAmount = entity.TotalAmount;
        dto.IsPosted = entity.IsPosted;
        dto.IsActive = entity.IsActive;
        dto.CreatedAt = entity.CreatedAt;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, DebitNoteDto dto)
    {
        var entity = await _context.DebitNotes
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted)
            throw new Exception(
                "Posted debit note cannot be updated.");

        entity.ReferenceNumber = dto.ReferenceNumber?.Trim();
        entity.Reason = dto.Reason?.Trim();

        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        var entity = await _context.DebitNotes
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            throw new Exception("Debit note not found.");

        if (entity.IsPosted)
            throw new Exception("Debit note is already posted.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x => x.Id == entity.FiscalYearId);

        if (fiscalYear == null || fiscalYear.IsClosed)
            throw new Exception("Fiscal year is closed.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == entity.FiscalYearPeriodId);

        if (period == null || period.IsClosed)
            throw new Exception("Fiscal year period is closed.");

        var lineCount = await _context.DebitNoteLines
            .CountAsync(x => x.DebitNoteId == id);

        if (lineCount == 0)
            throw new Exception(
                "Debit note must have at least one line before posting.");

        var total = await _context.DebitNoteLines
            .Where(x => x.DebitNoteId == id)
            .SumAsync(x => x.LineTotal);

        if (total <= 0)
            throw new Exception(
                "Debit note total must be greater than zero.");

        entity.TotalAmount = total;
        entity.IsPosted = true;
        entity.PostedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.DebitNotes
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted)
            throw new Exception(
                "Posted debit note cannot be deleted.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
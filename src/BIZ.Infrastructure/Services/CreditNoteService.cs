using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Application.Services;

public class CreditNoteService : ICreditNoteService
{
    private readonly TenantDbContext _context;

    public CreditNoteService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<CreditNoteDto>> GetAllAsync()
    {
        return await _context.CreditNotes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new CreditNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CreditNoteNumber = x.CreditNoteNumber,
                CreditNoteDate = x.CreditNoteDate,
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

    public async Task<CreditNoteDto?> GetByIdAsync(int id)
    {
        return await _context.CreditNotes
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new CreditNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CreditNoteNumber = x.CreditNoteNumber,
                CreditNoteDate = x.CreditNoteDate,
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

    public async Task<CreditNoteDto?> GetByNumberAsync(string creditNoteNumber)
    {
        creditNoteNumber = creditNoteNumber.Trim().ToUpper();

        return await _context.CreditNotes
            .AsNoTracking()
            .Where(x =>
                x.CreditNoteNumber == creditNoteNumber &&
                x.IsActive)
            .Select(x => new CreditNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CreditNoteNumber = x.CreditNoteNumber,
                CreditNoteDate = x.CreditNoteDate,
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

    public async Task<List<CreditNoteDto>> GetByFiscalYearAsync(int fiscalYearId)
    {
        return await _context.CreditNotes
            .AsNoTracking()
            .Where(x => x.FiscalYearId == fiscalYearId && x.IsActive)
            .OrderByDescending(x => x.CreditNoteDate)
            .Select(x => new CreditNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CreditNoteNumber = x.CreditNoteNumber,
                CreditNoteDate = x.CreditNoteDate,
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

    public async Task<List<CreditNoteDto>> GetByPeriodAsync(int fiscalYearPeriodId)
    {
        return await _context.CreditNotes
            .AsNoTracking()
            .Where(x =>
                x.FiscalYearPeriodId == fiscalYearPeriodId &&
                x.IsActive)
            .OrderByDescending(x => x.CreditNoteDate)
            .Select(x => new CreditNoteDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                LedgerAccountId = x.LedgerAccountId,
                SubLedgerId = x.SubLedgerId,
                CreditNoteNumber = x.CreditNoteNumber,
                CreditNoteDate = x.CreditNoteDate,
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

    public async Task<CreditNoteDto> CreateAsync(CreditNoteDto dto)
    {
        var number = dto.CreditNoteNumber.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(number))
            throw new Exception("Credit note number is required.");

        if (dto.CreditNoteDate == default)
            throw new Exception("Credit note date is required.");

        if (dto.TotalAmount <= 0)
            throw new Exception("Total amount must be greater than zero.");

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
            throw new Exception("Fiscal year period does not belong to selected fiscal year.");

        if (dto.CreditNoteDate < period.StartDate ||
            dto.CreditNoteDate > period.EndDate)
            throw new Exception("Credit note date must be within fiscal year period.");

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
                throw new Exception("Sub ledger does not belong to selected ledger account.");
        }

        var exists = await _context.CreditNotes
            .AnyAsync(x =>
                x.CreditNoteNumber == number &&
                x.IsActive);

        if (exists)
            throw new Exception("Credit note number already exists.");

        var entity = new CreditNote
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            LedgerAccountId = dto.LedgerAccountId,
            SubLedgerId = dto.SubLedgerId,
            CreditNoteNumber = number,
            CreditNoteDate = dto.CreditNoteDate,
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Reason = dto.Reason?.Trim(),
            TotalAmount = dto.TotalAmount,
            IsPosted = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.CreditNotes.Add(entity);
        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.CreditNoteNumber = entity.CreditNoteNumber;
        dto.IsPosted = entity.IsPosted;
        dto.IsActive = entity.IsActive;
        dto.CreatedAt = entity.CreatedAt;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, CreditNoteDto dto)
    {
        var entity = await _context.CreditNotes
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted)
            throw new Exception("Posted credit note cannot be updated.");

        if (dto.TotalAmount <= 0)
            throw new Exception("Total amount must be greater than zero.");

        entity.ReferenceNumber = dto.ReferenceNumber?.Trim();
        entity.Reason = dto.Reason?.Trim();
        entity.TotalAmount = dto.TotalAmount;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        var entity = await _context.CreditNotes
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            throw new Exception("Credit note not found.");

        if (entity.IsPosted)
            throw new Exception("Credit note is already posted.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x => x.Id == entity.FiscalYearId);

        if (fiscalYear == null || fiscalYear.IsClosed)
            throw new Exception("Fiscal year is closed.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x => x.Id == entity.FiscalYearPeriodId);

        if (period == null || period.IsClosed)
            throw new Exception("Fiscal year period is closed.");

        var lineCount = await _context.CreditNoteLines
            .CountAsync(x => x.CreditNoteId == id);

        if (lineCount == 0)
            throw new Exception("Credit note must have at least one line before posting.");

        entity.IsPosted = true;
        entity.PostedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.CreditNotes
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted)
            throw new Exception("Posted credit note cannot be deleted.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
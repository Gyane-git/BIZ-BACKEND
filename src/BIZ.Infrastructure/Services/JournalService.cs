using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class JournalService : IJournalService
{
    private readonly TenantDbContext _context;

    public JournalService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<JournalDto>> GetAllAsync()
    {
        return await _context.Journals
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.JournalDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new JournalDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                JournalNumber = x.JournalNumber,
                JournalDate = x.JournalDate,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                JournalType = x.JournalType,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<JournalDto?> GetByIdAsync(int id)
    {
        return await _context.Journals
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.IsActive)
            .Select(x => new JournalDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                JournalNumber = x.JournalNumber,
                JournalDate = x.JournalDate,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                JournalType = x.JournalType,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<JournalDto?> GetByNumberAsync(
        string journalNumber)
    {
        journalNumber = journalNumber.Trim().ToUpper();

        return await _context.Journals
            .AsNoTracking()
            .Where(x =>
                x.JournalNumber == journalNumber &&
                x.IsActive)
            .Select(x => new JournalDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                JournalNumber = x.JournalNumber,
                JournalDate = x.JournalDate,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                JournalType = x.JournalType,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<JournalDto>> GetByFiscalYearAsync(
        int fiscalYearId)
    {
        return await _context.Journals
            .AsNoTracking()
            .Where(x =>
                x.FiscalYearId == fiscalYearId &&
                x.IsActive)
            .OrderByDescending(x => x.JournalDate)
            .Select(x => new JournalDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                JournalNumber = x.JournalNumber,
                JournalDate = x.JournalDate,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                JournalType = x.JournalType,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<JournalDto>> GetByPeriodAsync(
        int fiscalYearPeriodId)
    {
        return await _context.Journals
            .AsNoTracking()
            .Where(x =>
                x.FiscalYearPeriodId == fiscalYearPeriodId &&
                x.IsActive)
            .OrderByDescending(x => x.JournalDate)
            .Select(x => new JournalDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                FiscalYearPeriodId = x.FiscalYearPeriodId,
                JournalNumber = x.JournalNumber,
                JournalDate = x.JournalDate,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                JournalType = x.JournalType,
                IsPosted = x.IsPosted,
                PostedAt = x.PostedAt,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<JournalDto> CreateAsync(
        JournalDto dto)
    {
        dto.JournalNumber =
            dto.JournalNumber.Trim().ToUpper();

        dto.JournalType =
            string.IsNullOrWhiteSpace(dto.JournalType)
                ? "General"
                : dto.JournalType.Trim();

        if (string.IsNullOrWhiteSpace(dto.JournalNumber))
            throw new Exception(
                "Journal Number is required.");

        if (dto.JournalDate == default)
            throw new Exception(
                "Journal Date is required.");

        if (dto.IsPosted)
            throw new Exception(
                "New Journal cannot be created as posted.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new Exception(
                "Active Fiscal Year not found.");

        if (fiscalYear.IsClosed)
            throw new Exception(
                "Cannot create Journal in a closed Fiscal Year.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.FiscalYearId == dto.FiscalYearId &&
                x.IsActive);

        if (period == null)
            throw new Exception(
                "Active Fiscal Year Period not found.");

        if (period.IsClosed)
            throw new Exception(
                "Cannot create Journal in a closed Period.");

        if (dto.JournalDate < period.StartDate ||
            dto.JournalDate > period.EndDate)
        {
            throw new Exception(
                "Journal Date must be within the Fiscal Year Period.");
        }

        var duplicateNumber =
            await _context.Journals.AnyAsync(x =>
                x.JournalNumber == dto.JournalNumber &&
                x.IsActive);

        if (duplicateNumber)
            throw new Exception(
                "Journal Number already exists.");

        var entity = new Journal
        {
            FiscalYearId = dto.FiscalYearId,
            FiscalYearPeriodId = dto.FiscalYearPeriodId,
            JournalNumber = dto.JournalNumber,
            JournalDate = dto.JournalDate,
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Description = dto.Description?.Trim(),
            JournalType = dto.JournalType,
            IsPosted = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Journals.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.IsPosted = entity.IsPosted;
        dto.IsActive = entity.IsActive;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        JournalDto dto)
    {
        var entity = await _context.Journals
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted)
            throw new Exception(
                "Posted Journal cannot be updated.");

        dto.JournalNumber =
            dto.JournalNumber.Trim().ToUpper();

        if (string.IsNullOrWhiteSpace(dto.JournalNumber))
            throw new Exception(
                "Journal Number is required.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new Exception(
                "Active Fiscal Year not found.");

        if (fiscalYear.IsClosed)
            throw new Exception(
                "Cannot update Journal in a closed Fiscal Year.");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearPeriodId &&
                x.FiscalYearId == dto.FiscalYearId &&
                x.IsActive);

        if (period == null)
            throw new Exception(
                "Active Fiscal Year Period not found.");

        if (period.IsClosed)
            throw new Exception(
                "Cannot update Journal in a closed Period.");

        if (dto.JournalDate < period.StartDate ||
            dto.JournalDate > period.EndDate)
        {
            throw new Exception(
                "Journal Date must be within the Fiscal Year Period.");
        }

        var duplicateNumber =
            await _context.Journals.AnyAsync(x =>
                x.Id != id &&
                x.JournalNumber == dto.JournalNumber &&
                x.IsActive);

        if (duplicateNumber)
            throw new Exception(
                "Journal Number already exists.");

        entity.FiscalYearId = dto.FiscalYearId;
        entity.FiscalYearPeriodId = dto.FiscalYearPeriodId;
        entity.JournalNumber = dto.JournalNumber;
        entity.JournalDate = dto.JournalDate;
        entity.ReferenceNumber = dto.ReferenceNumber?.Trim();
        entity.Description = dto.Description?.Trim();
        entity.JournalType =
            string.IsNullOrWhiteSpace(dto.JournalType)
                ? "General"
                : dto.JournalType.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostAsync(int id)
    {
        var entity = await _context.Journals
            .Include(x => x.JournalLines)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted)
            return true;

        if (entity.JournalLines.Count < 2)
            throw new Exception(
                "Journal must contain at least two lines before posting.");

        var totalDebit = entity.JournalLines
            .Sum(x => x.Debit);

        var totalCredit = entity.JournalLines
            .Sum(x => x.Credit);

        if (totalDebit <= 0)
            throw new Exception(
                "Journal Debit total must be greater than zero.");

        if (totalDebit != totalCredit)
            throw new Exception(
                $"Journal is not balanced. Debit: {totalDebit}, Credit: {totalCredit}");

        var period = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == entity.FiscalYearPeriodId &&
                x.IsActive);

        if (period == null)
            throw new Exception(
                "Fiscal Year Period not found.");

        if (period.IsClosed)
            throw new Exception(
                "Cannot post Journal in a closed Period.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == entity.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new Exception(
                "Fiscal Year not found.");

        if (fiscalYear.IsClosed)
            throw new Exception(
                "Cannot post Journal in a closed Fiscal Year.");

        entity.IsPosted = true;
        entity.PostedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Journals
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsPosted)
            throw new Exception(
                "Posted Journal cannot be deleted.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
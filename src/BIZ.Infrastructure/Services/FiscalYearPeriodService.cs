using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class FiscalYearPeriodService : IFiscalYearPeriodService
{
    private readonly TenantDbContext _context;

    public FiscalYearPeriodService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<FiscalYearPeriodDto>> GetAllAsync()
    {
        return await _context.FiscalYearPeriods
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FiscalYearId)
            .ThenBy(x => x.PeriodNumber)
            .Select(x => new FiscalYearPeriodDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                PeriodNumber = x.PeriodNumber,
                Code = x.Code,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsCurrent = x.IsCurrent,
                IsClosed = x.IsClosed,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<FiscalYearPeriodDto>> GetByFiscalYearAsync(
        int fiscalYearId)
    {
        return await _context.FiscalYearPeriods
            .AsNoTracking()
            .Where(x =>
                x.FiscalYearId == fiscalYearId &&
                x.IsActive)
            .OrderBy(x => x.PeriodNumber)
            .Select(x => new FiscalYearPeriodDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                PeriodNumber = x.PeriodNumber,
                Code = x.Code,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsCurrent = x.IsCurrent,
                IsClosed = x.IsClosed,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<FiscalYearPeriodDto?> GetByIdAsync(int id)
    {
        return await _context.FiscalYearPeriods
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new FiscalYearPeriodDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                PeriodNumber = x.PeriodNumber,
                Code = x.Code,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsCurrent = x.IsCurrent,
                IsClosed = x.IsClosed,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<FiscalYearPeriodDto?> GetByCodeAsync(string code)
    {
        code = code.Trim().ToUpper();

        return await _context.FiscalYearPeriods
            .AsNoTracking()
            .Where(x =>
                x.Code == code &&
                x.IsActive)
            .Select(x => new FiscalYearPeriodDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                PeriodNumber = x.PeriodNumber,
                Code = x.Code,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsCurrent = x.IsCurrent,
                IsClosed = x.IsClosed,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<FiscalYearPeriodDto?> GetCurrentAsync(
        int fiscalYearId)
    {
        return await _context.FiscalYearPeriods
            .AsNoTracking()
            .Where(x =>
                x.FiscalYearId == fiscalYearId &&
                x.IsCurrent &&
                x.IsActive &&
                !x.IsClosed)
            .Select(x => new FiscalYearPeriodDto
            {
                Id = x.Id,
                FiscalYearId = x.FiscalYearId,
                PeriodNumber = x.PeriodNumber,
                Code = x.Code,
                Name = x.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsCurrent = x.IsCurrent,
                IsClosed = x.IsClosed,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<FiscalYearPeriodDto> CreateAsync(
        FiscalYearPeriodDto dto)
    {
        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new Exception("Period Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Period Name is required.");

        if (dto.PeriodNumber <= 0)
            throw new Exception("Period Number must be greater than 0.");

        if (dto.StartDate >= dto.EndDate)
            throw new Exception(
                "Start Date must be earlier than End Date.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new Exception(
                "Active Fiscal Year not found.");

        if (fiscalYear.IsClosed)
            throw new Exception(
                "Cannot add period to a closed Fiscal Year.");

        if (dto.StartDate < fiscalYear.StartDate ||
            dto.EndDate > fiscalYear.EndDate)
        {
            throw new Exception(
                "Period dates must be within the Fiscal Year dates.");
        }

        var duplicateCode = await _context.FiscalYearPeriods
            .AnyAsync(x =>
                x.Code == dto.Code &&
                x.IsActive);

        if (duplicateCode)
            throw new Exception(
                "Fiscal Year Period Code already exists.");

        var duplicateNumber = await _context.FiscalYearPeriods
            .AnyAsync(x =>
                x.FiscalYearId == dto.FiscalYearId &&
                x.PeriodNumber == dto.PeriodNumber &&
                x.IsActive);

        if (duplicateNumber)
            throw new Exception(
                "Period Number already exists in this Fiscal Year.");

        var overlap = await _context.FiscalYearPeriods
            .AnyAsync(x =>
                x.FiscalYearId == dto.FiscalYearId &&
                x.IsActive &&
                dto.StartDate <= x.EndDate &&
                dto.EndDate >= x.StartDate);

        if (overlap)
            throw new Exception(
                "Period dates overlap with an existing period.");

        if (dto.IsCurrent)
        {
            var currentPeriods = await _context.FiscalYearPeriods
                .Where(x =>
                    x.FiscalYearId == dto.FiscalYearId &&
                    x.IsActive &&
                    x.IsCurrent)
                .ToListAsync();

            foreach (var period in currentPeriods)
                period.IsCurrent = false;
        }

        var entity = new FiscalYearPeriod
        {
            FiscalYearId = dto.FiscalYearId,
            PeriodNumber = dto.PeriodNumber,
            Code = dto.Code,
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsCurrent = dto.IsCurrent,
            IsClosed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.FiscalYearPeriods.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.IsClosed = entity.IsClosed;
        dto.IsActive = entity.IsActive;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        FiscalYearPeriodDto dto)
    {
        var entity = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsClosed)
            throw new Exception(
                "Closed Fiscal Year Period cannot be updated.");

        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new Exception("Period Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Period Name is required.");

        if (dto.PeriodNumber <= 0)
            throw new Exception(
                "Period Number must be greater than 0.");

        if (dto.StartDate >= dto.EndDate)
            throw new Exception(
                "Start Date must be earlier than End Date.");

        var fiscalYear = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == dto.FiscalYearId &&
                x.IsActive);

        if (fiscalYear == null)
            throw new Exception(
                "Active Fiscal Year not found.");

        if (fiscalYear.IsClosed)
            throw new Exception(
                "Cannot update period of a closed Fiscal Year.");

        if (dto.StartDate < fiscalYear.StartDate ||
            dto.EndDate > fiscalYear.EndDate)
        {
            throw new Exception(
                "Period dates must be within the Fiscal Year dates.");
        }

        var duplicateCode = await _context.FiscalYearPeriods
            .AnyAsync(x =>
                x.Id != id &&
                x.Code == dto.Code &&
                x.IsActive);

        if (duplicateCode)
            throw new Exception(
                "Fiscal Year Period Code already exists.");

        var duplicateNumber = await _context.FiscalYearPeriods
            .AnyAsync(x =>
                x.Id != id &&
                x.FiscalYearId == dto.FiscalYearId &&
                x.PeriodNumber == dto.PeriodNumber &&
                x.IsActive);

        if (duplicateNumber)
            throw new Exception(
                "Period Number already exists in this Fiscal Year.");

        var overlap = await _context.FiscalYearPeriods
            .AnyAsync(x =>
                x.Id != id &&
                x.FiscalYearId == dto.FiscalYearId &&
                x.IsActive &&
                dto.StartDate <= x.EndDate &&
                dto.EndDate >= x.StartDate);

        if (overlap)
            throw new Exception(
                "Period dates overlap with an existing period.");

        if (dto.IsCurrent)
        {
            var currentPeriods = await _context.FiscalYearPeriods
                .Where(x =>
                    x.Id != id &&
                    x.FiscalYearId == dto.FiscalYearId &&
                    x.IsActive &&
                    x.IsCurrent)
                .ToListAsync();

            foreach (var period in currentPeriods)
                period.IsCurrent = false;
        }

        entity.FiscalYearId = dto.FiscalYearId;
        entity.PeriodNumber = dto.PeriodNumber;
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.IsCurrent = dto.IsCurrent;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CloseAsync(int id)
    {
        var entity = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsClosed)
            return true;

        entity.IsClosed = true;
        entity.IsCurrent = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.FiscalYearPeriods
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsClosed)
            throw new Exception(
                "Closed Fiscal Year Period cannot be deleted.");

        entity.IsActive = false;
        entity.IsCurrent = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
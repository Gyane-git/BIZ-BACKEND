using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class FiscalYearService : IFiscalYearService
{
    private readonly TenantDbContext _context;

    public FiscalYearService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<FiscalYearDto>> GetAllAsync()
    {
        return await _context.FiscalYears
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new FiscalYearDto
            {
                Id = x.Id,
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

    public async Task<FiscalYearDto?> GetByIdAsync(int id)
    {
        return await _context.FiscalYears
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new FiscalYearDto
            {
                Id = x.Id,
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

    public async Task<FiscalYearDto?> GetByCodeAsync(string code)
    {
        code = code.Trim().ToUpper();

        return await _context.FiscalYears
            .AsNoTracking()
            .Where(x =>
                x.Code == code &&
                x.IsActive)
            .Select(x => new FiscalYearDto
            {
                Id = x.Id,
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

    public async Task<FiscalYearDto?> GetCurrentAsync()
    {
        return await _context.FiscalYears
            .AsNoTracking()
            .Where(x =>
                x.IsCurrent &&
                x.IsActive &&
                !x.IsClosed)
            .Select(x => new FiscalYearDto
            {
                Id = x.Id,
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

    public async Task<FiscalYearDto> CreateAsync(
        FiscalYearDto dto)
    {
        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new Exception("Fiscal Year Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Fiscal Year Name is required.");

        if (dto.StartDate >= dto.EndDate)
            throw new Exception(
                "Start Date must be earlier than End Date.");

        if (dto.IsClosed)
            throw new Exception(
                "New Fiscal Year cannot be created as closed.");

        var duplicateCode = await _context.FiscalYears
            .AnyAsync(x =>
                x.Code == dto.Code &&
                x.IsActive);

        if (duplicateCode)
            throw new Exception(
                "Fiscal Year Code already exists.");

        var duplicateName = await _context.FiscalYears
            .AnyAsync(x =>
                x.Name == dto.Name &&
                x.IsActive);

        if (duplicateName)
            throw new Exception(
                "Fiscal Year Name already exists.");

        var overlap = await _context.FiscalYears
            .AnyAsync(x =>
                x.IsActive &&
                dto.StartDate <= x.EndDate &&
                dto.EndDate >= x.StartDate);

        if (overlap)
            throw new Exception(
                "Fiscal Year dates overlap with an existing Fiscal Year.");

        if (dto.IsCurrent)
        {
            var currentYears = await _context.FiscalYears
                .Where(x =>
                    x.IsActive &&
                    x.IsCurrent)
                .ToListAsync();

            foreach (var year in currentYears)
                year.IsCurrent = false;
        }

        var entity = new FiscalYear
        {
            Code = dto.Code,
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsCurrent = dto.IsCurrent,
            IsClosed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.FiscalYears.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.IsClosed = entity.IsClosed;
        dto.IsActive = entity.IsActive;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        FiscalYearDto dto)
    {
        var entity = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsClosed)
            throw new Exception(
                "Closed Fiscal Year cannot be updated.");

        dto.Code = dto.Code.Trim().ToUpper();
        dto.Name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new Exception("Fiscal Year Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new Exception("Fiscal Year Name is required.");

        if (dto.StartDate >= dto.EndDate)
            throw new Exception(
                "Start Date must be earlier than End Date.");

        var duplicateCode = await _context.FiscalYears
            .AnyAsync(x =>
                x.Id != id &&
                x.Code == dto.Code &&
                x.IsActive);

        if (duplicateCode)
            throw new Exception(
                "Fiscal Year Code already exists.");

        var duplicateName = await _context.FiscalYears
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == dto.Name &&
                x.IsActive);

        if (duplicateName)
            throw new Exception(
                "Fiscal Year Name already exists.");

        var overlap = await _context.FiscalYears
            .AnyAsync(x =>
                x.Id != id &&
                x.IsActive &&
                dto.StartDate <= x.EndDate &&
                dto.EndDate >= x.StartDate);

        if (overlap)
            throw new Exception(
                "Fiscal Year dates overlap with an existing Fiscal Year.");

        if (dto.IsCurrent)
        {
            var currentYears = await _context.FiscalYears
                .Where(x =>
                    x.Id != id &&
                    x.IsActive &&
                    x.IsCurrent)
                .ToListAsync();

            foreach (var year in currentYears)
                year.IsCurrent = false;
        }

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
        var entity = await _context.FiscalYears
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
        var entity = await _context.FiscalYears
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);

        if (entity == null)
            return false;

        if (entity.IsClosed)
            throw new Exception(
                "Closed Fiscal Year cannot be deleted.");

        if (entity.IsCurrent)
            throw new Exception(
                "Current Fiscal Year cannot be deleted. Close it first.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
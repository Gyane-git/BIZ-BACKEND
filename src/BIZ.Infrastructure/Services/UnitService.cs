using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class UnitService : IUnitService
{
    private readonly TenantDbContext _db;

    public UnitService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<UnitDto>> GetAllAsync()
    {
        return await _db.Units
            .AsNoTracking()
            .Select(x => new UnitDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Symbol = x.Symbol,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<UnitDto?> GetByIdAsync(int id)
    {
        return await _db.Units
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UnitDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Symbol = x.Symbol,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UnitDto> CreateAsync(UnitDto dto)
    {
        var exists = await _db.Units
            .AnyAsync(x => x.Code == dto.Code);

        if (exists)
            throw new InvalidOperationException(
                $"Unit code '{dto.Code}' already exists.");

        var unit = new Unit
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Symbol = dto.Symbol?.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Units.Add(unit);

        await _db.SaveChangesAsync();

        dto.Id = unit.Id;
        dto.CreatedAt = unit.CreatedAt;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, UnitDto dto)
    {
        var unit = await _db.Units
            .FirstOrDefaultAsync(x => x.Id == id);

        if (unit == null)
            return false;

        var duplicate = await _db.Units
            .AnyAsync(x =>
                x.Code == dto.Code &&
                x.Id != id);

        if (duplicate)
            throw new InvalidOperationException(
                $"Unit code '{dto.Code}' already exists.");

        unit.Code = dto.Code.Trim();
        unit.Name = dto.Name.Trim();
        unit.Symbol = dto.Symbol?.Trim();
        unit.Description = dto.Description?.Trim();
        unit.IsActive = dto.IsActive;
        unit.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var unit = await _db.Units
            .FirstOrDefaultAsync(x => x.Id == id);

        if (unit == null)
            return false;

        _db.Units.Remove(unit);

        await _db.SaveChangesAsync();

        return true;
    }
}
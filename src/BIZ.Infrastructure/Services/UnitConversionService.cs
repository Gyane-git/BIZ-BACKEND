using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class UnitConversionService : IUnitConversionService
{
    private readonly TenantDbContext _db;

    public UnitConversionService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<UnitConversionDto>> GetAllAsync()
    {
        return await _db.UnitConversions
            .AsNoTracking()
            .Select(x => new UnitConversionDto
            {
                Id = x.Id,
                FromUnitId = x.FromUnitId,
                ToUnitId = x.ToUnitId,
                ConversionFactor = x.ConversionFactor,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<UnitConversionDto?> GetByIdAsync(int id)
    {
        return await _db.UnitConversions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UnitConversionDto
            {
                Id = x.Id,
                FromUnitId = x.FromUnitId,
                ToUnitId = x.ToUnitId,
                ConversionFactor = x.ConversionFactor,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UnitConversionDto> CreateAsync(UnitConversionDto dto)
    {
        if (dto.FromUnitId == dto.ToUnitId)
            throw new InvalidOperationException(
                "From Unit and To Unit cannot be the same.");

        if (dto.ConversionFactor <= 0)
            throw new InvalidOperationException(
                "Conversion factor must be greater than zero.");

        var fromUnitExists = await _db.Units
            .AnyAsync(x => x.Id == dto.FromUnitId && x.IsActive);

        if (!fromUnitExists)
            throw new InvalidOperationException(
                "From Unit not found or inactive.");

        var toUnitExists = await _db.Units
            .AnyAsync(x => x.Id == dto.ToUnitId && x.IsActive);

        if (!toUnitExists)
            throw new InvalidOperationException(
                "To Unit not found or inactive.");

        var exists = await _db.UnitConversions
            .AnyAsync(x =>
                x.FromUnitId == dto.FromUnitId &&
                x.ToUnitId == dto.ToUnitId);

        if (exists)
            throw new InvalidOperationException(
                "This unit conversion already exists.");

        var conversion = new UnitConversion
        {
            FromUnitId = dto.FromUnitId,
            ToUnitId = dto.ToUnitId,
            ConversionFactor = dto.ConversionFactor,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.UnitConversions.Add(conversion);

        await _db.SaveChangesAsync();

        dto.Id = conversion.Id;
        dto.CreatedAt = conversion.CreatedAt;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        UnitConversionDto dto)
    {
        var conversion = await _db.UnitConversions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (conversion == null)
            return false;

        if (dto.FromUnitId == dto.ToUnitId)
            throw new InvalidOperationException(
                "From Unit and To Unit cannot be the same.");

        if (dto.ConversionFactor <= 0)
            throw new InvalidOperationException(
                "Conversion factor must be greater than zero.");

        var duplicate = await _db.UnitConversions
            .AnyAsync(x =>
                x.FromUnitId == dto.FromUnitId &&
                x.ToUnitId == dto.ToUnitId &&
                x.Id != id);

        if (duplicate)
            throw new InvalidOperationException(
                "This unit conversion already exists.");

        conversion.FromUnitId = dto.FromUnitId;
        conversion.ToUnitId = dto.ToUnitId;
        conversion.ConversionFactor = dto.ConversionFactor;
        conversion.IsActive = dto.IsActive;
        conversion.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var conversion = await _db.UnitConversions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (conversion == null)
            return false;

        _db.UnitConversions.Remove(conversion);

        await _db.SaveChangesAsync();

        return true;
    }
}
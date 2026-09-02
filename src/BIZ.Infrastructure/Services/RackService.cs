using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class RackService : IRackService
{
    private readonly TenantDbContext _db;

    public RackService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<RackDto>> GetAllAsync()
    {
        return await _db.Racks
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RackDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                WarehouseId = x.WarehouseId,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<RackDto?> GetByIdAsync(int id)
    {
        return await _db.Racks
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new RackDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                WarehouseId = x.WarehouseId,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<RackDto> CreateAsync(RackDto dto)
    {
        var code = dto.Code.Trim();

        var exists = await _db.Racks
            .AnyAsync(x => x.Code == code);

        if (exists)
            throw new InvalidOperationException(
                "Rack code already exists.");

        if (dto.WarehouseId.HasValue)
        {
            var warehouseExists = await _db.Warehouses
                .AnyAsync(x => x.Id == dto.WarehouseId.Value);

            if (!warehouseExists)
                throw new InvalidOperationException(
                    "Warehouse not found.");
        }

        var entity = new Rack
        {
            Code = code,
            Name = dto.Name.Trim(),
            WarehouseId = dto.WarehouseId,
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Racks.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.Code = entity.Code;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        RackDto dto)
    {
        var entity = await _db.Racks
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var code = dto.Code.Trim();

        var exists = await _db.Racks
            .AnyAsync(x =>
                x.Code == code &&
                x.Id != id);

        if (exists)
            throw new InvalidOperationException(
                "Rack code already exists.");

        if (dto.WarehouseId.HasValue)
        {
            var warehouseExists = await _db.Warehouses
                .AnyAsync(x =>
                    x.Id == dto.WarehouseId.Value);

            if (!warehouseExists)
                throw new InvalidOperationException(
                    "Warehouse not found.");
        }

        entity.Code = code;
        entity.Name = dto.Name.Trim();
        entity.WarehouseId = dto.WarehouseId;
        entity.Description = dto.Description?.Trim();
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Racks
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}
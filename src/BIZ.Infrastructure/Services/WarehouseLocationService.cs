using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class WarehouseLocationService
    : IWarehouseLocationService
{
    private readonly TenantDbContext _db;

    public WarehouseLocationService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<WarehouseLocationDto>> GetAllAsync()
    {
        return await _db.WarehouseLocations
            .AsNoTracking()
            .OrderBy(x => x.WarehouseId)
            .ThenBy(x => x.Sequence)
            .Select(x => new WarehouseLocationDto
            {
                Id = x.Id,
                WarehouseId = x.WarehouseId,
                Location = x.Location,
                SubLocation = x.SubLocation,
                Rack = x.Rack,
                Col = x.Col,
                ActualLocation = x.ActualLocation,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                Memo = x.Memo,
                Sequence = x.Sequence,
                LocCode = x.LocCode,
                Pcode = x.Pcode
            })
            .ToListAsync();
    }

    public async Task<List<WarehouseLocationDto>>
        GetByWarehouseAsync(int warehouseId)
    {
        return await _db.WarehouseLocations
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId)
            .OrderBy(x => x.Sequence)
            .Select(x => new WarehouseLocationDto
            {
                Id = x.Id,
                WarehouseId = x.WarehouseId,
                Location = x.Location,
                SubLocation = x.SubLocation,
                Rack = x.Rack,
                Col = x.Col,
                ActualLocation = x.ActualLocation,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                Memo = x.Memo,
                Sequence = x.Sequence,
                LocCode = x.LocCode,
                Pcode = x.Pcode
            })
            .ToListAsync();
    }

    public async Task<WarehouseLocationDto?>
        GetByIdAsync(int id)
    {
        return await _db.WarehouseLocations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new WarehouseLocationDto
            {
                Id = x.Id,
                WarehouseId = x.WarehouseId,
                Location = x.Location,
                SubLocation = x.SubLocation,
                Rack = x.Rack,
                Col = x.Col,
                ActualLocation = x.ActualLocation,
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,
                Memo = x.Memo,
                Sequence = x.Sequence,
                LocCode = x.LocCode,
                Pcode = x.Pcode
            })
            .FirstOrDefaultAsync();
    }

    public async Task<WarehouseLocationDto>
        CreateAsync(WarehouseLocationDto dto)
    {
        var warehouseExists = await _db.Warehouses
            .AnyAsync(x => x.Id == dto.WarehouseId);

        if (!warehouseExists)
            throw new InvalidOperationException(
                "Warehouse not found.");

        var entity = new WarehouseLocation
        {
            WarehouseId = dto.WarehouseId,
            Location = dto.Location?.Trim(),
            SubLocation = dto.SubLocation?.Trim(),
            Rack = dto.Rack?.Trim(),
            Col = dto.Col?.Trim(),
            ActualLocation = dto.ActualLocation?.Trim(),
            CreatedBy = dto.CreatedBy?.Trim(),
            CreatedDate = dto.CreatedDate ?? DateTime.UtcNow,
            Memo = dto.Memo?.Trim(),
            Sequence = dto.Sequence,
            LocCode = dto.LocCode?.Trim(),
            Pcode = dto.Pcode?.Trim()
        };

        _db.WarehouseLocations.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        WarehouseLocationDto dto)
    {
        var entity = await _db.WarehouseLocations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var warehouseExists = await _db.Warehouses
            .AnyAsync(x => x.Id == dto.WarehouseId);

        if (!warehouseExists)
            throw new InvalidOperationException(
                "Warehouse not found.");

        entity.WarehouseId = dto.WarehouseId;
        entity.Location = dto.Location?.Trim();
        entity.SubLocation = dto.SubLocation?.Trim();
        entity.Rack = dto.Rack?.Trim();
        entity.Col = dto.Col?.Trim();
        entity.ActualLocation = dto.ActualLocation?.Trim();
        entity.CreatedBy = dto.CreatedBy?.Trim();
        entity.CreatedDate = dto.CreatedDate;
        entity.Memo = dto.Memo?.Trim();
        entity.Sequence = dto.Sequence;
        entity.LocCode = dto.LocCode?.Trim();
        entity.Pcode = dto.Pcode?.Trim();

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.WarehouseLocations
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        _db.WarehouseLocations.Remove(entity);

        await _db.SaveChangesAsync();

        return true;
    }
}
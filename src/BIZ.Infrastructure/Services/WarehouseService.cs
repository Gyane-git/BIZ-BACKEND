using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly TenantDbContext _db;

    public WarehouseService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<WarehouseDto>> GetAllAsync()
    {
        return await _db.Warehouses
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new WarehouseDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                ShortName = x.ShortName,
                City = x.City,
                Address = x.Address,
                TelNo = x.TelNo,
                MobileNo = x.MobileNo,
                ContactPerson = x.ContactPerson,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<WarehouseDto?> GetByIdAsync(int id)
    {
        return await _db.Warehouses
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new WarehouseDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                ShortName = x.ShortName,
                City = x.City,
                Address = x.Address,
                TelNo = x.TelNo,
                MobileNo = x.MobileNo,
                ContactPerson = x.ContactPerson,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<WarehouseDto> CreateAsync(
        WarehouseDto dto)
    {
        var code = dto.Code.Trim();

        var exists = await _db.Warehouses
            .AnyAsync(x => x.Code == code);

        if (exists)
            throw new InvalidOperationException(
                "Warehouse code already exists.");

        var entity = new Warehouse
        {
            Code = code,
            Name = dto.Name?.Trim(),
            ShortName = dto.ShortName?.Trim(),
            City = dto.City?.Trim(),
            Address = dto.Address?.Trim(),
            TelNo = dto.TelNo?.Trim(),
            MobileNo = dto.MobileNo?.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Warehouses.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.Code = entity.Code;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        WarehouseDto dto)
    {
        var entity = await _db.Warehouses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var code = dto.Code.Trim();

        var exists = await _db.Warehouses
            .AnyAsync(x =>
                x.Code == code &&
                x.Id != id);

        if (exists)
            throw new InvalidOperationException(
                "Warehouse code already exists.");

        entity.Code = code;
        entity.Name = dto.Name?.Trim();
        entity.ShortName = dto.ShortName?.Trim();
        entity.City = dto.City?.Trim();
        entity.Address = dto.Address?.Trim();
        entity.TelNo = dto.TelNo?.Trim();
        entity.MobileNo = dto.MobileNo?.Trim();
        entity.ContactPerson = dto.ContactPerson?.Trim();
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Warehouses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        // Soft delete
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}
using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductSubGroupService : IProductSubGroupService
{
    private readonly TenantDbContext _db;

    public ProductSubGroupService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ProductSubGroupDto>> GetAllAsync()
    {
        return await _db.ProductSubGroups
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductSubGroupDto
            {
                ProductGroupId = x.ProductGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductSubGroupDto>> GetByGroupIdAsync(
        int productGroupId)
    {
        return await _db.ProductSubGroups
            .AsNoTracking()
            .Where(x => x.ProductGroupId == productGroupId)
            .OrderBy(x => x.Name)
            .Select(x => new ProductSubGroupDto
            {
                ProductGroupId = x.ProductGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductSubGroupDto?> GetByIdAsync(int id)
    {
        return await _db.ProductSubGroups
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductSubGroupDto
            {
                ProductGroupId = x.ProductGroupId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductSubGroupDto> CreateAsync(
        ProductSubGroupDto dto)
    {
        var groupExists = await _db.ProductGroups
            .AnyAsync(x =>
                x.Id == dto.ProductGroupId &&
                x.IsActive);

        if (!groupExists)
            throw new InvalidOperationException(
                "Product group not found or inactive.");

        var code = dto.Code.Trim();

        var exists = await _db.ProductSubGroups
            .AnyAsync(x =>
                x.ProductGroupId == dto.ProductGroupId &&
                x.Code == code);

        if (exists)
            throw new InvalidOperationException(
                $"Product sub-group code '{code}' already exists in this group.");

        var entity = new ProductSubGroup
        {
            ProductGroupId = dto.ProductGroupId,
            Code = code,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductSubGroups.Add(entity);

        await _db.SaveChangesAsync();

        return new ProductSubGroupDto
        {
            ProductGroupId = entity.ProductGroupId,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductSubGroupDto dto)
    {
        var entity = await _db.ProductSubGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        var groupExists = await _db.ProductGroups
            .AnyAsync(x =>
                x.Id == dto.ProductGroupId &&
                x.IsActive);

        if (!groupExists)
            throw new InvalidOperationException(
                "Product group not found or inactive.");

        var code = dto.Code.Trim();

        var duplicate = await _db.ProductSubGroups
            .AnyAsync(x =>
                x.ProductGroupId == dto.ProductGroupId &&
                x.Code == code &&
                x.Id != id);

        if (duplicate)
            throw new InvalidOperationException(
                $"Product sub-group code '{code}' already exists in this group.");

        entity.ProductGroupId = dto.ProductGroupId;
        entity.Code = code;
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductSubGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        _db.ProductSubGroups.Remove(entity);

        await _db.SaveChangesAsync();

        return true;
    }
}
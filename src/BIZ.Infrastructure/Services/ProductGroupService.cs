using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductGroupService : IProductGroupService
{
    private readonly TenantDbContext _db;

    public ProductGroupService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ProductGroupDto>> GetAllAsync()
    {
        return await _db.ProductGroups
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductGroupDto
            {
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductGroupDto?> GetByIdAsync(int id)
    {
        return await _db.ProductGroups
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductGroupDto
            {
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductGroupDto> CreateAsync(ProductGroupDto dto)
    {
        var code = dto.Code.Trim();

        var exists = await _db.ProductGroups
            .AnyAsync(x => x.Code == code);

        if (exists)
            throw new InvalidOperationException(
                $"Product group code '{code}' already exists.");

        var entity = new ProductGroup
        {
            Code = code,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductGroups.Add(entity);

        await _db.SaveChangesAsync();

        return new ProductGroupDto
        {
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductGroupDto dto)
    {
        var entity = await _db.ProductGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        var code = dto.Code.Trim();

        var duplicate = await _db.ProductGroups
            .AnyAsync(x => x.Code == code && x.Id != id);

        if (duplicate)
            throw new InvalidOperationException(
                $"Product group code '{code}' already exists.");

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
        var entity = await _db.ProductGroups
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        var hasSubGroups = await _db.ProductSubGroups
            .AnyAsync(x => x.ProductGroupId == id);

        if (hasSubGroups)
            throw new InvalidOperationException(
                "Cannot delete product group because it has product sub-groups.");

        _db.ProductGroups.Remove(entity);

        await _db.SaveChangesAsync();

        return true;
    }
}
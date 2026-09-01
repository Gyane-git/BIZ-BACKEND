using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly TenantDbContext _db;

    public ProductCategoryService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ProductCategoryDto>> GetAllAsync()
    {
        return await _db.ProductCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductCategoryDto
            {
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductCategoryDto?> GetByIdAsync(int id)
    {
        return await _db.ProductCategories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductCategoryDto
            {
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductCategoryDto> CreateAsync(ProductCategoryDto dto)
    {
        var exists = await _db.ProductCategories
            .AnyAsync(x => x.Code == dto.Code);

        if (exists)
            throw new InvalidOperationException(
                $"Product category code '{dto.Code}' already exists.");

        var entity = new ProductCategory
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductCategories.Add(entity);

        await _db.SaveChangesAsync();

        dto.Code = entity.Code;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductCategoryDto dto)
    {
        var entity = await _db.ProductCategories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        var duplicate = await _db.ProductCategories
            .AnyAsync(x =>
                x.Code == dto.Code &&
                x.Id != id);

        if (duplicate)
            throw new InvalidOperationException(
                $"Product category code '{dto.Code}' already exists.");

        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductCategories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        _db.ProductCategories.Remove(entity);

        await _db.SaveChangesAsync();

        return true;
    }
}
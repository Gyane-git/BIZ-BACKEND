using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly TenantDbContext _db;

    public BrandService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<BrandDto>> GetAllAsync()
    {
        return await _db.Brands
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BrandDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<BrandDto?> GetByIdAsync(int id)
    {
        return await _db.Brands
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BrandDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BrandDto> CreateAsync(BrandDto dto)
    {
        var code = dto.Code.Trim();

        var exists = await _db.Brands
            .AnyAsync(x => x.Code == code);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Brand code '{code}' already exists."
            );
        }

        var brand = new Brand
        {
            Code = code,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Brands.Add(brand);

        await _db.SaveChangesAsync();

        dto.Id = brand.Id;
        dto.Code = brand.Code;
        dto.Name = brand.Name;
        dto.Description = brand.Description;
        dto.IsActive = brand.IsActive;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, BrandDto dto)
    {
        var brand = await _db.Brands
            .FirstOrDefaultAsync(x => x.Id == id);

        if (brand is null)
            return false;

        var code = dto.Code.Trim();

        var duplicate = await _db.Brands
            .AnyAsync(x =>
                x.Code == code &&
                x.Id != id);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Brand code '{code}' already exists."
            );
        }

        brand.Code = code;
        brand.Name = dto.Name.Trim();
        brand.Description = dto.Description?.Trim();
        brand.IsActive = dto.IsActive;
        brand.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var brand = await _db.Brands
            .FirstOrDefaultAsync(x => x.Id == id);

        if (brand is null)
            return false;

        _db.Brands.Remove(brand);

        await _db.SaveChangesAsync();

        return true;
    }
}
using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ModelService : IModelService
{
    private readonly TenantDbContext _db;

    public ModelService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<ModelDto>> GetAllAsync()
    {
        return await _db.Models
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ModelDto
            {
                Id = x.Id,
                BrandId = x.BrandId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ModelDto?> GetByIdAsync(int id)
    {
        return await _db.Models
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ModelDto
            {
                Id = x.Id,
                BrandId = x.BrandId,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ModelDto> CreateAsync(ModelDto dto)
    {
        var brandExists = await _db.Brands
            .AnyAsync(x => x.Id == dto.BrandId && x.IsActive);

        if (!brandExists)
        {
            throw new InvalidOperationException(
                $"Brand with id {dto.BrandId} was not found or is inactive."
            );
        }

        var code = dto.Code.Trim();

        var exists = await _db.Models
            .AnyAsync(x =>
                x.BrandId == dto.BrandId &&
                x.Code == code);

        if (exists)
        {
            throw new InvalidOperationException(
                $"Model code '{code}' already exists for this brand."
            );
        }

        var model = new Model
        {
            BrandId = dto.BrandId,
            Code = code,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Models.Add(model);

        await _db.SaveChangesAsync();

        return new ModelDto
        {
            Id = model.Id,
            BrandId = model.BrandId,
            Code = model.Code,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        };
    }

    public async Task<bool> UpdateAsync(int id, ModelDto dto)
    {
        var model = await _db.Models
            .FirstOrDefaultAsync(x => x.Id == id);

        if (model is null)
            return false;

        var brandExists = await _db.Brands
            .AnyAsync(x => x.Id == dto.BrandId && x.IsActive);

        if (!brandExists)
        {
            throw new InvalidOperationException(
                $"Brand with id {dto.BrandId} was not found or is inactive."
            );
        }

        var code = dto.Code.Trim();

        var duplicate = await _db.Models
            .AnyAsync(x =>
                x.BrandId == dto.BrandId &&
                x.Code == code &&
                x.Id != id);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Model code '{code}' already exists for this brand."
            );
        }

        model.BrandId = dto.BrandId;
        model.Code = code;
        model.Name = dto.Name.Trim();
        model.Description = dto.Description?.Trim();
        model.IsActive = dto.IsActive;
        model.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var model = await _db.Models
            .FirstOrDefaultAsync(x => x.Id == id);

        if (model is null)
            return false;

        _db.Models.Remove(model);

        await _db.SaveChangesAsync();

        return true;
    }
}
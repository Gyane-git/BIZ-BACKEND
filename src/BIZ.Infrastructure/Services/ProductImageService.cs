using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductImageService : IProductImageService
{
    private readonly TenantDbContext _db;

    public ProductImageService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductImageDto>> GetAllAsync()
    {
        return await _db.ProductImages
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.DisplayOrder)
            .Select(x => new ProductImageDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ImageUrl = x.ImageUrl,
                AltText = x.AltText,
                IsPrimary = x.IsPrimary,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductImageDto>> GetByProductAsync(
        int productId)
    {
        return await _db.ProductImages
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.IsActive)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.DisplayOrder)
            .Select(x => new ProductImageDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ImageUrl = x.ImageUrl,
                AltText = x.AltText,
                IsPrimary = x.IsPrimary,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductImageDto?> GetByIdAsync(int id)
    {
        return await _db.ProductImages
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductImageDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ImageUrl = x.ImageUrl,
                AltText = x.AltText,
                IsPrimary = x.IsPrimary,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductImageDto> CreateAsync(
        ProductImageDto dto)
    {
        dto.ImageUrl = dto.ImageUrl.Trim();

        if (string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            throw new InvalidOperationException(
                "ImageUrl is required.");
        }

        // Check Product
        var productExists = await _db.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
        {
            throw new InvalidOperationException(
                $"Product with ID {dto.ProductId} was not found or is inactive.");
        }

        // Display order cannot be negative
        if (dto.DisplayOrder < 0)
        {
            throw new InvalidOperationException(
                "DisplayOrder cannot be negative.");
        }

        // Only one primary image
        if (dto.IsPrimary)
        {
            var existingPrimary = await _db.ProductImages
                .Where(x =>
                    x.ProductId == dto.ProductId &&
                    x.IsPrimary &&
                    x.IsActive)
                .ToListAsync();

            foreach (var item in existingPrimary)
            {
                item.IsPrimary = false;
            }
        }

        var entity = new ProductImage
        {
            ProductId = dto.ProductId,
            ImageUrl = dto.ImageUrl,
            AltText = dto.AltText?.Trim(),
            IsPrimary = dto.IsPrimary,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductImages.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductImageDto dto)
    {
        dto.ImageUrl = dto.ImageUrl.Trim();

        if (string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            throw new InvalidOperationException(
                "ImageUrl is required.");
        }

        var entity = await _db.ProductImages
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        // Check Product
        var productExists = await _db.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
        {
            throw new InvalidOperationException(
                $"Product with ID {dto.ProductId} was not found or is inactive.");
        }

        if (dto.DisplayOrder < 0)
        {
            throw new InvalidOperationException(
                "DisplayOrder cannot be negative.");
        }

        // Only one primary image
        if (dto.IsPrimary)
        {
            var existingPrimary = await _db.ProductImages
                .Where(x =>
                    x.Id != id &&
                    x.ProductId == dto.ProductId &&
                    x.IsPrimary &&
                    x.IsActive)
                .ToListAsync();

            foreach (var item in existingPrimary)
            {
                item.IsPrimary = false;
            }
        }

        entity.ProductId = dto.ProductId;
        entity.ImageUrl = dto.ImageUrl;
        entity.AltText = dto.AltText?.Trim();
        entity.IsPrimary = dto.IsPrimary;
        entity.DisplayOrder = dto.DisplayOrder;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductImages
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        // Soft delete
        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}
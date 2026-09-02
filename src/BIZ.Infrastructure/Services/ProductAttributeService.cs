using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductAttributeService : IProductAttributeService
{
    private readonly TenantDbContext _db;

    public ProductAttributeService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductAttributeDto>> GetAllAsync()
    {
        return await _db.ProductAttributes
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.AttributeName)
            .Select(x => new ProductAttributeDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                AttributeName = x.AttributeName,
                AttributeValue = x.AttributeValue,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductAttributeDto>> GetByProductAsync(
        int productId)
    {
        return await _db.ProductAttributes
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.IsActive)
            .OrderBy(x => x.AttributeName)
            .Select(x => new ProductAttributeDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                AttributeName = x.AttributeName,
                AttributeValue = x.AttributeValue,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductAttributeDto?> GetByIdAsync(
        int id)
    {
        return await _db.ProductAttributes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductAttributeDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                AttributeName = x.AttributeName,
                AttributeValue = x.AttributeValue,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductAttributeDto> CreateAsync(
        ProductAttributeDto dto)
    {
        dto.AttributeName = dto.AttributeName.Trim();
        dto.AttributeValue = dto.AttributeValue.Trim();

        if (string.IsNullOrWhiteSpace(dto.AttributeName))
        {
            throw new InvalidOperationException(
                "AttributeName is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.AttributeValue))
        {
            throw new InvalidOperationException(
                "AttributeValue is required.");
        }

        var productExists = await _db.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
        {
            throw new InvalidOperationException(
                $"Product with ID {dto.ProductId} was not found or inactive.");
        }

        var duplicate = await _db.ProductAttributes
            .AnyAsync(x =>
                x.ProductId == dto.ProductId &&
                x.AttributeName == dto.AttributeName &&
                x.IsActive);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Attribute '{dto.AttributeName}' already exists for this product.");
        }

        var entity = new ProductAttribute
        {
            ProductId = dto.ProductId,
            AttributeName = dto.AttributeName,
            AttributeValue = dto.AttributeValue,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductAttributes.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductAttributeDto dto)
    {
        dto.AttributeName = dto.AttributeName.Trim();
        dto.AttributeValue = dto.AttributeValue.Trim();

        if (string.IsNullOrWhiteSpace(dto.AttributeName))
        {
            throw new InvalidOperationException(
                "AttributeName is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.AttributeValue))
        {
            throw new InvalidOperationException(
                "AttributeValue is required.");
        }

        var entity = await _db.ProductAttributes
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return false;
        }

        var productExists = await _db.Products
            .AnyAsync(x =>
                x.Id == dto.ProductId &&
                x.IsActive);

        if (!productExists)
        {
            throw new InvalidOperationException(
                $"Product with ID {dto.ProductId} was not found or inactive.");
        }

        var duplicate = await _db.ProductAttributes
            .AnyAsync(x =>
                x.Id != id &&
                x.ProductId == dto.ProductId &&
                x.AttributeName == dto.AttributeName &&
                x.IsActive);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Attribute '{dto.AttributeName}' already exists for this product.");
        }

        entity.ProductId = dto.ProductId;
        entity.AttributeName = dto.AttributeName;
        entity.AttributeValue = dto.AttributeValue;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductAttributes
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            return false;
        }

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}
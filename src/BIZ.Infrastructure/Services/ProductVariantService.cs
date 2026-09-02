using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductVariantService : IProductVariantService
{
    private readonly TenantDbContext _db;

    public ProductVariantService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductVariantDto>> GetAllAsync()
    {
        return await _db.ProductVariants
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.VariantCode)
            .Select(x => new ProductVariantDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                VariantCode = x.VariantCode,
                VariantName = x.VariantName,
                Color = x.Color,
                Size = x.Size,
                Specification = x.Specification,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                DealerPrice = x.DealerPrice,
                DiscountRate = x.DiscountRate,
                ReorderLevel = x.ReorderLevel,
                ReorderQty = x.ReorderQty,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductVariantDto>> GetByProductAsync(
        int productId)
    {
        return await _db.ProductVariants
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.IsActive)
            .OrderBy(x => x.VariantCode)
            .Select(x => new ProductVariantDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                VariantCode = x.VariantCode,
                VariantName = x.VariantName,
                Color = x.Color,
                Size = x.Size,
                Specification = x.Specification,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                DealerPrice = x.DealerPrice,
                DiscountRate = x.DiscountRate,
                ReorderLevel = x.ReorderLevel,
                ReorderQty = x.ReorderQty,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductVariantDto?> GetByIdAsync(
        int id)
    {
        return await _db.ProductVariants
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductVariantDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                VariantCode = x.VariantCode,
                VariantName = x.VariantName,
                Color = x.Color,
                Size = x.Size,
                Specification = x.Specification,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                DealerPrice = x.DealerPrice,
                DiscountRate = x.DiscountRate,
                ReorderLevel = x.ReorderLevel,
                ReorderQty = x.ReorderQty,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductVariantDto?> GetByCodeAsync(
        string variantCode)
    {
        variantCode = variantCode.Trim();

        return await _db.ProductVariants
            .AsNoTracking()
            .Where(x =>
                x.VariantCode == variantCode)
            .Select(x => new ProductVariantDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                VariantCode = x.VariantCode,
                VariantName = x.VariantName,
                Color = x.Color,
                Size = x.Size,
                Specification = x.Specification,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                DealerPrice = x.DealerPrice,
                DiscountRate = x.DiscountRate,
                ReorderLevel = x.ReorderLevel,
                ReorderQty = x.ReorderQty,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductVariantDto> CreateAsync(
        ProductVariantDto dto)
    {
        dto.VariantCode = dto.VariantCode.Trim();
        dto.VariantName = dto.VariantName.Trim();
        dto.Color = dto.Color?.Trim();
        dto.Size = dto.Size?.Trim();
        dto.Specification = dto.Specification?.Trim();

        if (string.IsNullOrWhiteSpace(dto.VariantCode))
        {
            throw new InvalidOperationException(
                "VariantCode is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.VariantName))
        {
            throw new InvalidOperationException(
                "VariantName is required.");
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

        var codeExists = await _db.ProductVariants
            .AnyAsync(x =>
                x.VariantCode == dto.VariantCode);

        if (codeExists)
        {
            throw new InvalidOperationException(
                $"VariantCode '{dto.VariantCode}' already exists.");
        }

        ValidateRates(dto);

        var entity = new ProductVariant
        {
            ProductId = dto.ProductId,
            VariantCode = dto.VariantCode,
            VariantName = dto.VariantName,
            Color = dto.Color,
            Size = dto.Size,
            Specification = dto.Specification,
            PurchaseRate = dto.PurchaseRate,
            SalesRate = dto.SalesRate,
            MRP = dto.MRP,
            DealerPrice = dto.DealerPrice,
            DiscountRate = dto.DiscountRate,
            ReorderLevel = dto.ReorderLevel,
            ReorderQty = dto.ReorderQty,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductVariants.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductVariantDto dto)
    {
        dto.VariantCode = dto.VariantCode.Trim();
        dto.VariantName = dto.VariantName.Trim();
        dto.Color = dto.Color?.Trim();
        dto.Size = dto.Size?.Trim();
        dto.Specification = dto.Specification?.Trim();

        if (string.IsNullOrWhiteSpace(dto.VariantCode))
        {
            throw new InvalidOperationException(
                "VariantCode is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.VariantName))
        {
            throw new InvalidOperationException(
                "VariantName is required.");
        }

        var entity = await _db.ProductVariants
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

        var codeExists = await _db.ProductVariants
            .AnyAsync(x =>
                x.Id != id &&
                x.VariantCode == dto.VariantCode);

        if (codeExists)
        {
            throw new InvalidOperationException(
                $"VariantCode '{dto.VariantCode}' already exists.");
        }

        ValidateRates(dto);

        entity.ProductId = dto.ProductId;
        entity.VariantCode = dto.VariantCode;
        entity.VariantName = dto.VariantName;
        entity.Color = dto.Color;
        entity.Size = dto.Size;
        entity.Specification = dto.Specification;
        entity.PurchaseRate = dto.PurchaseRate;
        entity.SalesRate = dto.SalesRate;
        entity.MRP = dto.MRP;
        entity.DealerPrice = dto.DealerPrice;
        entity.DiscountRate = dto.DiscountRate;
        entity.ReorderLevel = dto.ReorderLevel;
        entity.ReorderQty = dto.ReorderQty;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductVariants
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

    private static void ValidateRates(ProductVariantDto dto)
    {
        if (dto.PurchaseRate < 0)
        {
            throw new InvalidOperationException(
                "PurchaseRate cannot be negative.");
        }

        if (dto.SalesRate < 0)
        {
            throw new InvalidOperationException(
                "SalesRate cannot be negative.");
        }

        if (dto.MRP < 0)
        {
            throw new InvalidOperationException(
                "MRP cannot be negative.");
        }

        if (dto.DealerPrice < 0)
        {
            throw new InvalidOperationException(
                "DealerPrice cannot be negative.");
        }

        if (dto.DiscountRate < 0)
        {
            throw new InvalidOperationException(
                "DiscountRate cannot be negative.");
        }

        if (dto.ReorderLevel < 0)
        {
            throw new InvalidOperationException(
                "ReorderLevel cannot be negative.");
        }

        if (dto.ReorderQty < 0)
        {
            throw new InvalidOperationException(
                "ReorderQty cannot be negative.");
        }
    }
}
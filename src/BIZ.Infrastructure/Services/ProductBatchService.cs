using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductBatchService : IProductBatchService
{
    private readonly TenantDbContext _db;

    public ProductBatchService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductBatchDto>> GetAllAsync()
    {
        return await _db.ProductBatches
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.BatchNumber)
            .Select(x => new ProductBatchDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                BatchNumber = x.BatchNumber,
                ManufacturingDate = x.ManufacturingDate,
                ExpiryDate = x.ExpiryDate,
                OpeningQuantity = x.OpeningQuantity,
                CurrentQuantity = x.CurrentQuantity,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductBatchDto>> GetByProductAsync(
        int productId)
    {
        return await _db.ProductBatches
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.IsActive)
            .OrderBy(x => x.BatchNumber)
            .Select(x => new ProductBatchDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                BatchNumber = x.BatchNumber,
                ManufacturingDate = x.ManufacturingDate,
                ExpiryDate = x.ExpiryDate,
                OpeningQuantity = x.OpeningQuantity,
                CurrentQuantity = x.CurrentQuantity,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductBatchDto>> GetByVariantAsync(
        int productVariantId)
    {
        return await _db.ProductBatches
            .AsNoTracking()
            .Where(x =>
                x.ProductVariantId == productVariantId &&
                x.IsActive)
            .OrderBy(x => x.BatchNumber)
            .Select(x => new ProductBatchDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                BatchNumber = x.BatchNumber,
                ManufacturingDate = x.ManufacturingDate,
                ExpiryDate = x.ExpiryDate,
                OpeningQuantity = x.OpeningQuantity,
                CurrentQuantity = x.CurrentQuantity,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductBatchDto?> GetByIdAsync(
        int id)
    {
        return await _db.ProductBatches
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductBatchDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                BatchNumber = x.BatchNumber,
                ManufacturingDate = x.ManufacturingDate,
                ExpiryDate = x.ExpiryDate,
                OpeningQuantity = x.OpeningQuantity,
                CurrentQuantity = x.CurrentQuantity,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductBatchDto?> GetByBatchNumberAsync(
        string batchNumber)
    {
        batchNumber = batchNumber.Trim();

        return await _db.ProductBatches
            .AsNoTracking()
            .Where(x =>
                x.BatchNumber == batchNumber)
            .Select(x => new ProductBatchDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                BatchNumber = x.BatchNumber,
                ManufacturingDate = x.ManufacturingDate,
                ExpiryDate = x.ExpiryDate,
                OpeningQuantity = x.OpeningQuantity,
                CurrentQuantity = x.CurrentQuantity,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductBatchDto> CreateAsync(
        ProductBatchDto dto)
    {
        dto.BatchNumber = dto.BatchNumber.Trim();

        if (string.IsNullOrWhiteSpace(dto.BatchNumber))
        {
            throw new InvalidOperationException(
                "BatchNumber is required.");
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

        if (dto.ProductVariantId.HasValue)
        {
            var variantExists = await _db.ProductVariants
                .AnyAsync(x =>
                    x.Id == dto.ProductVariantId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (!variantExists)
            {
                throw new InvalidOperationException(
                    "ProductVariant was not found, inactive, or does not belong to the selected product.");
            }
        }

        if (dto.ManufacturingDate.HasValue &&
            dto.ExpiryDate.HasValue &&
            dto.ExpiryDate < dto.ManufacturingDate)
        {
            throw new InvalidOperationException(
                "ExpiryDate cannot be earlier than ManufacturingDate.");
        }

        if (dto.OpeningQuantity < 0)
        {
            throw new InvalidOperationException(
                "OpeningQuantity cannot be negative.");
        }

        if (dto.CurrentQuantity < 0)
        {
            throw new InvalidOperationException(
                "CurrentQuantity cannot be negative.");
        }

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

        var duplicate = await _db.ProductBatches
            .AnyAsync(x =>
                x.ProductId == dto.ProductId &&
                x.BatchNumber == dto.BatchNumber);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"BatchNumber '{dto.BatchNumber}' already exists for this product.");
        }

        var entity = new ProductBatch
        {
            ProductId = dto.ProductId,
            ProductVariantId = dto.ProductVariantId,
            BatchNumber = dto.BatchNumber,
            ManufacturingDate = dto.ManufacturingDate,
            ExpiryDate = dto.ExpiryDate,
            OpeningQuantity = dto.OpeningQuantity,
            CurrentQuantity = dto.CurrentQuantity,
            PurchaseRate = dto.PurchaseRate,
            SalesRate = dto.SalesRate,
            MRP = dto.MRP,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductBatches.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductBatchDto dto)
    {
        dto.BatchNumber = dto.BatchNumber.Trim();

        if (string.IsNullOrWhiteSpace(dto.BatchNumber))
        {
            throw new InvalidOperationException(
                "BatchNumber is required.");
        }

        var entity = await _db.ProductBatches
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

        if (dto.ProductVariantId.HasValue)
        {
            var variantExists = await _db.ProductVariants
                .AnyAsync(x =>
                    x.Id == dto.ProductVariantId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (!variantExists)
            {
                throw new InvalidOperationException(
                    "ProductVariant was not found, inactive, or does not belong to the selected product.");
            }
        }

        if (dto.ManufacturingDate.HasValue &&
            dto.ExpiryDate.HasValue &&
            dto.ExpiryDate < dto.ManufacturingDate)
        {
            throw new InvalidOperationException(
                "ExpiryDate cannot be earlier than ManufacturingDate.");
        }

        if (dto.OpeningQuantity < 0)
        {
            throw new InvalidOperationException(
                "OpeningQuantity cannot be negative.");
        }

        if (dto.CurrentQuantity < 0)
        {
            throw new InvalidOperationException(
                "CurrentQuantity cannot be negative.");
        }

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

        var duplicate = await _db.ProductBatches
            .AnyAsync(x =>
                x.Id != id &&
                x.ProductId == dto.ProductId &&
                x.BatchNumber == dto.BatchNumber);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"BatchNumber '{dto.BatchNumber}' already exists for this product.");
        }

        entity.ProductId = dto.ProductId;
        entity.ProductVariantId = dto.ProductVariantId;
        entity.BatchNumber = dto.BatchNumber;
        entity.ManufacturingDate = dto.ManufacturingDate;
        entity.ExpiryDate = dto.ExpiryDate;
        entity.OpeningQuantity = dto.OpeningQuantity;
        entity.CurrentQuantity = dto.CurrentQuantity;
        entity.PurchaseRate = dto.PurchaseRate;
        entity.SalesRate = dto.SalesRate;
        entity.MRP = dto.MRP;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductBatches
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
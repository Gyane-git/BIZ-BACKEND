using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductBarcodeService : IProductBarcodeService
{
    private readonly TenantDbContext _db;

    public ProductBarcodeService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductBarcodeDto>> GetAllAsync()
    {
        return await _db.ProductBarcodes
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.Barcode)
            .Select(x => new ProductBarcodeDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductUnitId = x.ProductUnitId,
                Barcode = x.Barcode,
                IsPrimary = x.IsPrimary,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductBarcodeDto>> GetByProductAsync(
        int productId)
    {
        return await _db.ProductBarcodes
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Barcode)
            .Select(x => new ProductBarcodeDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductUnitId = x.ProductUnitId,
                Barcode = x.Barcode,
                IsPrimary = x.IsPrimary,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductBarcodeDto?> GetByIdAsync(int id)
    {
        return await _db.ProductBarcodes
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductBarcodeDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductUnitId = x.ProductUnitId,
                Barcode = x.Barcode,
                IsPrimary = x.IsPrimary,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductBarcodeDto?> GetByBarcodeAsync(
        string barcode)
    {
        barcode = barcode.Trim();

        return await _db.ProductBarcodes
            .AsNoTracking()
            .Where(x =>
                x.Barcode == barcode &&
                x.IsActive)
            .Select(x => new ProductBarcodeDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductUnitId = x.ProductUnitId,
                Barcode = x.Barcode,
                IsPrimary = x.IsPrimary,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductBarcodeDto> CreateAsync(
        ProductBarcodeDto dto)
    {
        dto.Barcode = dto.Barcode.Trim();

        if (string.IsNullOrWhiteSpace(dto.Barcode))
        {
            throw new InvalidOperationException(
                "Barcode is required.");
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

        // Check ProductUnit if provided
        if (dto.ProductUnitId.HasValue)
        {
            var productUnitExists = await _db.ProductUnits
                .AnyAsync(x =>
                    x.Id == dto.ProductUnitId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (!productUnitExists)
            {
                throw new InvalidOperationException(
                    "ProductUnit was not found, inactive, or does not belong to this product.");
            }
        }

        // Barcode must be unique
        var duplicate = await _db.ProductBarcodes
            .AnyAsync(x =>
                x.Barcode == dto.Barcode &&
                x.IsActive);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Barcode '{dto.Barcode}' already exists.");
        }

        // Only one primary barcode per product
        if (dto.IsPrimary)
        {
            var existingPrimary = await _db.ProductBarcodes
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

        var entity = new ProductBarcode
        {
            ProductId = dto.ProductId,
            ProductUnitId = dto.ProductUnitId,
            Barcode = dto.Barcode,
            IsPrimary = dto.IsPrimary,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.ProductBarcodes.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductBarcodeDto dto)
    {
        dto.Barcode = dto.Barcode.Trim();

        if (string.IsNullOrWhiteSpace(dto.Barcode))
        {
            throw new InvalidOperationException(
                "Barcode is required.");
        }

        var entity = await _db.ProductBarcodes
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

        // Check ProductUnit
        if (dto.ProductUnitId.HasValue)
        {
            var productUnitExists = await _db.ProductUnits
                .AnyAsync(x =>
                    x.Id == dto.ProductUnitId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (!productUnitExists)
            {
                throw new InvalidOperationException(
                    "ProductUnit was not found, inactive, or does not belong to this product.");
            }
        }

        // Duplicate barcode
        var duplicate = await _db.ProductBarcodes
            .AnyAsync(x =>
                x.Id != id &&
                x.Barcode == dto.Barcode &&
                x.IsActive);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Barcode '{dto.Barcode}' already exists.");
        }

        // Only one primary barcode
        if (dto.IsPrimary)
        {
            var existingPrimary = await _db.ProductBarcodes
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
        entity.ProductUnitId = dto.ProductUnitId;
        entity.Barcode = dto.Barcode;
        entity.IsPrimary = dto.IsPrimary;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductBarcodes
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
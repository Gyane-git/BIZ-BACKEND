using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductUnitService : IProductUnitService
{
    private readonly TenantDbContext _db;

    public ProductUnitService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductUnitDto>> GetAllAsync()
    {
        return await _db.ProductUnits
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.UnitId)
            .Select(x => new ProductUnitDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                ConversionQuantity = x.ConversionQuantity,
                IsBaseUnit = x.IsBaseUnit,
                IsPurchaseUnit = x.IsPurchaseUnit,
                IsSalesUnit = x.IsSalesUnit,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<ProductUnitDto>> GetByProductAsync(int productId)
    {
        return await _db.ProductUnits
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.UnitId)
            .Select(x => new ProductUnitDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                ConversionQuantity = x.ConversionQuantity,
                IsBaseUnit = x.IsBaseUnit,
                IsPurchaseUnit = x.IsPurchaseUnit,
                IsSalesUnit = x.IsSalesUnit,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductUnitDto?> GetByIdAsync(int id)
    {
        return await _db.ProductUnits
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductUnitDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                UnitId = x.UnitId,
                ConversionQuantity = x.ConversionQuantity,
                IsBaseUnit = x.IsBaseUnit,
                IsPurchaseUnit = x.IsPurchaseUnit,
                IsSalesUnit = x.IsSalesUnit,
                PurchaseRate = x.PurchaseRate,
                SalesRate = x.SalesRate,
                MRP = x.MRP,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductUnitDto> CreateAsync(ProductUnitDto dto)
    {
        // Validate Product
        var productExists = await _db.Products
            .AnyAsync(x => x.Id == dto.ProductId && x.IsActive);

        if (!productExists)
        {
            throw new InvalidOperationException(
                $"Product with ID {dto.ProductId} was not found or is inactive.");
        }

        // Validate Unit
        var unitExists = await _db.Units
            .AnyAsync(x => x.Id == dto.UnitId && x.IsActive);

        if (!unitExists)
        {
            throw new InvalidOperationException(
                $"Unit with ID {dto.UnitId} was not found or is inactive.");
        }

        // Conversion validation
        if (dto.ConversionQuantity <= 0)
        {
            throw new InvalidOperationException(
                "ConversionQuantity must be greater than zero.");
        }

        // Duplicate Product + Unit
        var duplicate = await _db.ProductUnits
            .AnyAsync(x =>
                x.ProductId == dto.ProductId &&
                x.UnitId == dto.UnitId &&
                x.IsActive);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "This Product and Unit combination already exists.");
        }

        // Only one Base Unit
        if (dto.IsBaseUnit)
        {
            var baseUnitExists = await _db.ProductUnits
                .AnyAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.IsBaseUnit &&
                    x.IsActive);

            if (baseUnitExists)
            {
                throw new InvalidOperationException(
                    "This product already has a base unit.");
            }
        }

        var entity = new ProductUnit
        {
            ProductId = dto.ProductId,
            UnitId = dto.UnitId,
            ConversionQuantity = dto.ConversionQuantity,
            IsBaseUnit = dto.IsBaseUnit,
            IsPurchaseUnit = dto.IsPurchaseUnit,
            IsSalesUnit = dto.IsSalesUnit,
            PurchaseRate = dto.PurchaseRate,
            SalesRate = dto.SalesRate,
            MRP = dto.MRP,
            IsActive = dto.IsActive
        };

        _db.ProductUnits.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, ProductUnitDto dto)
    {
        var entity = await _db.ProductUnits
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        // Validate Product
        var productExists = await _db.Products
            .AnyAsync(x => x.Id == dto.ProductId && x.IsActive);

        if (!productExists)
        {
            throw new InvalidOperationException(
                $"Product with ID {dto.ProductId} was not found or is inactive.");
        }

        // Validate Unit
        var unitExists = await _db.Units
            .AnyAsync(x => x.Id == dto.UnitId && x.IsActive);

        if (!unitExists)
        {
            throw new InvalidOperationException(
                $"Unit with ID {dto.UnitId} was not found or is inactive.");
        }

        if (dto.ConversionQuantity <= 0)
        {
            throw new InvalidOperationException(
                "ConversionQuantity must be greater than zero.");
        }

        // Duplicate Product + Unit
        var duplicate = await _db.ProductUnits
            .AnyAsync(x =>
                x.Id != id &&
                x.ProductId == dto.ProductId &&
                x.UnitId == dto.UnitId &&
                x.IsActive);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "This Product and Unit combination already exists.");
        }

        // Only one Base Unit
        if (dto.IsBaseUnit)
        {
            var baseUnitExists = await _db.ProductUnits
                .AnyAsync(x =>
                    x.Id != id &&
                    x.ProductId == dto.ProductId &&
                    x.IsBaseUnit &&
                    x.IsActive);

            if (baseUnitExists)
            {
                throw new InvalidOperationException(
                    "This product already has a base unit.");
            }
        }

        entity.ProductId = dto.ProductId;
        entity.UnitId = dto.UnitId;
        entity.ConversionQuantity = dto.ConversionQuantity;
        entity.IsBaseUnit = dto.IsBaseUnit;
        entity.IsPurchaseUnit = dto.IsPurchaseUnit;
        entity.IsSalesUnit = dto.IsSalesUnit;
        entity.PurchaseRate = dto.PurchaseRate;
        entity.SalesRate = dto.SalesRate;
        entity.MRP = dto.MRP;
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.ProductUnits
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        // Soft delete
        entity.IsActive = false;

        await _db.SaveChangesAsync();

        return true;
    }
}
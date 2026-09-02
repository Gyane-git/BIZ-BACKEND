using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductUnitService : IProductUnitService
{
    private readonly TenantDbContext _context;

    public ProductUnitService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductUnitDto>> GetAllAsync()
    {
        return await _context.ProductUnits
            .AsNoTracking()
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
        return await _context.ProductUnits
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
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
        return await _context.ProductUnits
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
        var productExists = await _context.Products
            .AnyAsync(x => x.Id == dto.ProductId && x.IsActive);

        if (!productExists)
            throw new KeyNotFoundException("Product not found.");

        var unitExists = await _context.Units
            .AnyAsync(x => x.Id == dto.UnitId && x.IsActive);

        if (!unitExists)
            throw new KeyNotFoundException("Unit not found.");

        if (dto.ConversionQuantity <= 0)
            throw new ArgumentException(
                "ConversionQuantity must be greater than 0.");

        var duplicate = await _context.ProductUnits
            .AnyAsync(x =>
                x.ProductId == dto.ProductId &&
                x.UnitId == dto.UnitId);

        if (duplicate)
            throw new InvalidOperationException(
                "This unit is already assigned to the product.");

        if (dto.IsBaseUnit)
        {
            var existingBase = await _context.ProductUnits
                .AnyAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.IsBaseUnit);

            if (existingBase)
                throw new InvalidOperationException(
                    "Product already has a base unit.");
        }

        var entity = new BIZ.Domain.Entities.ProductUnit
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

        _context.ProductUnits.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductUnitDto dto)
    {
        var entity = await _context.ProductUnits
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        if (dto.ConversionQuantity <= 0)
            throw new ArgumentException(
                "ConversionQuantity must be greater than 0.");

        var duplicate = await _context.ProductUnits
            .AnyAsync(x =>
                x.Id != id &&
                x.ProductId == dto.ProductId &&
                x.UnitId == dto.UnitId);

        if (duplicate)
            throw new InvalidOperationException(
                "This unit is already assigned to the product.");

        if (dto.IsBaseUnit)
        {
            var existingBase = await _context.ProductUnits
                .AnyAsync(x =>
                    x.Id != id &&
                    x.ProductId == dto.ProductId &&
                    x.IsBaseUnit);

            if (existingBase)
                throw new InvalidOperationException(
                    "Product already has another base unit.");
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

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.ProductUnits
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        entity.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }
}
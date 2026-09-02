using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly TenantDbContext _context;

    public ProductService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                ShortName = x.ShortName,

                Category = x.Category,
                ValuationMethod = x.ValuationMethod,

                ProductGroupCode = x.ProductGroupCode,
                ProductSubGroupCode = x.ProductSubGroupCode,

                MRP = x.MRP,
                TradeRate = x.TradeRate,
                BuyRate = x.BuyRate,
                SalesRate = x.SalesRate,
                DealerPrice = x.DealerPrice,
                DiscountRate = x.DiscountRate,
                Margin = x.Margin,

                Vat = x.Vat,
                ExciseRate = x.ExciseRate,
                BeforeVat = x.BeforeVat,

                MaxStock = x.MaxStock,
                ReorderLevel = x.ReorderLevel,
                ReorderQty = x.ReorderQty,

                CurrencyCode = x.CurrencyCode,

                HasBatch = x.HasBatch,
                HasExpiryDate = x.HasExpiryDate,
                HasManufacturingDate = x.HasManufacturingDate,

                IsFavourite = x.IsFavourite,
                IsInsurableItem = x.IsInsurableItem,
                IsRestaurantProduct = x.IsRestaurantProduct,

                ProductPoint = x.ProductPoint,
                HSCode = x.HSCode,

                PurchaseGLCode = x.PurchaseGLCode,
                PurchaseReturnGLCode = x.PurchaseReturnGLCode,
                SalesGLCode = x.SalesGLCode,
                SalesReturnGLCode = x.SalesReturnGLCode,

                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                ShortName = x.ShortName,

                Category = x.Category,
                ValuationMethod = x.ValuationMethod,

                ProductGroupCode = x.ProductGroupCode,
                ProductSubGroupCode = x.ProductSubGroupCode,

                MRP = x.MRP,
                TradeRate = x.TradeRate,
                BuyRate = x.BuyRate,
                SalesRate = x.SalesRate,
                DealerPrice = x.DealerPrice,
                DiscountRate = x.DiscountRate,
                Margin = x.Margin,

                Vat = x.Vat,
                ExciseRate = x.ExciseRate,
                BeforeVat = x.BeforeVat,

                MaxStock = x.MaxStock,
                ReorderLevel = x.ReorderLevel,
                ReorderQty = x.ReorderQty,

                CurrencyCode = x.CurrencyCode,

                HasBatch = x.HasBatch,
                HasExpiryDate = x.HasExpiryDate,
                HasManufacturingDate = x.HasManufacturingDate,

                IsFavourite = x.IsFavourite,
                IsInsurableItem = x.IsInsurableItem,
                IsRestaurantProduct = x.IsRestaurantProduct,

                ProductPoint = x.ProductPoint,
                HSCode = x.HSCode,

                PurchaseGLCode = x.PurchaseGLCode,
                PurchaseReturnGLCode = x.PurchaseReturnGLCode,
                SalesGLCode = x.SalesGLCode,
                SalesReturnGLCode = x.SalesReturnGLCode,

                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDto?> GetByCodeAsync(string code)
    {
        code = code.Trim();

        return await _context.Products
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                ShortName = x.ShortName,

                Category = x.Category,
                ValuationMethod = x.ValuationMethod,

                ProductGroupCode = x.ProductGroupCode,
                ProductSubGroupCode = x.ProductSubGroupCode,

                MRP = x.MRP,
                TradeRate = x.TradeRate,
                BuyRate = x.BuyRate,
                SalesRate = x.SalesRate,
                DealerPrice = x.DealerPrice,
                DiscountRate = x.DiscountRate,
                Margin = x.Margin,

                Vat = x.Vat,
                ExciseRate = x.ExciseRate,
                BeforeVat = x.BeforeVat,

                MaxStock = x.MaxStock,
                ReorderLevel = x.ReorderLevel,
                ReorderQty = x.ReorderQty,

                CurrencyCode = x.CurrencyCode,

                HasBatch = x.HasBatch,
                HasExpiryDate = x.HasExpiryDate,
                HasManufacturingDate = x.HasManufacturingDate,

                IsFavourite = x.IsFavourite,
                IsInsurableItem = x.IsInsurableItem,
                IsRestaurantProduct = x.IsRestaurantProduct,

                ProductPoint = x.ProductPoint,
                HSCode = x.HSCode,

                PurchaseGLCode = x.PurchaseGLCode,
                PurchaseReturnGLCode = x.PurchaseReturnGLCode,
                SalesGLCode = x.SalesGLCode,
                SalesReturnGLCode = x.SalesReturnGLCode,

                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDto> CreateAsync(ProductDto dto)
    {
        dto.Code = dto.Code.Trim();
        dto.Name = dto.Name.Trim();
        dto.ShortName = dto.ShortName.Trim();

        var codeExists = await _context.Products
            .AnyAsync(x => x.Code == dto.Code);

        if (codeExists)
            throw new InvalidOperationException(
                $"Product code '{dto.Code}' already exists.");

        var nameExists = await _context.Products
            .AnyAsync(x => x.Name == dto.Name);

        if (nameExists)
            throw new InvalidOperationException(
                $"Product name '{dto.Name}' already exists.");

        var entity = new Product
        {
            Code = dto.Code,
            Name = dto.Name,
            ShortName = dto.ShortName,

            Category = dto.Category,
            ValuationMethod = dto.ValuationMethod,

            ProductGroupCode = dto.ProductGroupCode,
            ProductSubGroupCode = dto.ProductSubGroupCode,

            MRP = dto.MRP,
            TradeRate = dto.TradeRate,
            BuyRate = dto.BuyRate,
            SalesRate = dto.SalesRate,
            DealerPrice = dto.DealerPrice,
            DiscountRate = dto.DiscountRate,
            Margin = dto.Margin,

            Vat = dto.Vat,
            ExciseRate = dto.ExciseRate,
            BeforeVat = dto.BeforeVat,

            MaxStock = dto.MaxStock,
            ReorderLevel = dto.ReorderLevel,
            ReorderQty = dto.ReorderQty,

            CurrencyCode = dto.CurrencyCode,

            HasBatch = dto.HasBatch,
            HasExpiryDate = dto.HasExpiryDate,
            HasManufacturingDate = dto.HasManufacturingDate,

            IsFavourite = dto.IsFavourite,
            IsInsurableItem = dto.IsInsurableItem,
            IsRestaurantProduct = dto.IsRestaurantProduct,

            ProductPoint = dto.ProductPoint,
            HSCode = dto.HSCode,

            PurchaseGLCode = dto.PurchaseGLCode,
            PurchaseReturnGLCode = dto.PurchaseReturnGLCode,
            SalesGLCode = dto.SalesGLCode,
            SalesReturnGLCode = dto.SalesReturnGLCode,

            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(entity);

        await _context.SaveChangesAsync();

        dto.Id = entity.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductDto dto)
    {
        var entity = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        dto.Code = dto.Code.Trim();
        dto.Name = dto.Name.Trim();
        dto.ShortName = dto.ShortName.Trim();

        var codeExists = await _context.Products
            .AnyAsync(x =>
                x.Id != id &&
                x.Code == dto.Code);

        if (codeExists)
            throw new InvalidOperationException(
                $"Product code '{dto.Code}' already exists.");

        var nameExists = await _context.Products
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == dto.Name);

        if (nameExists)
            throw new InvalidOperationException(
                $"Product name '{dto.Name}' already exists.");

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.ShortName = dto.ShortName;

        entity.Category = dto.Category;
        entity.ValuationMethod = dto.ValuationMethod;

        entity.ProductGroupCode = dto.ProductGroupCode;
        entity.ProductSubGroupCode = dto.ProductSubGroupCode;

        entity.MRP = dto.MRP;
        entity.TradeRate = dto.TradeRate;
        entity.BuyRate = dto.BuyRate;
        entity.SalesRate = dto.SalesRate;
        entity.DealerPrice = dto.DealerPrice;
        entity.DiscountRate = dto.DiscountRate;
        entity.Margin = dto.Margin;

        entity.Vat = dto.Vat;
        entity.ExciseRate = dto.ExciseRate;
        entity.BeforeVat = dto.BeforeVat;

        entity.MaxStock = dto.MaxStock;
        entity.ReorderLevel = dto.ReorderLevel;
        entity.ReorderQty = dto.ReorderQty;

        entity.CurrencyCode = dto.CurrencyCode;

        entity.HasBatch = dto.HasBatch;
        entity.HasExpiryDate = dto.HasExpiryDate;
        entity.HasManufacturingDate = dto.HasManufacturingDate;

        entity.IsFavourite = dto.IsFavourite;
        entity.IsInsurableItem = dto.IsInsurableItem;
        entity.IsRestaurantProduct = dto.IsRestaurantProduct;

        entity.ProductPoint = dto.ProductPoint;
        entity.HSCode = dto.HSCode;

        entity.PurchaseGLCode = dto.PurchaseGLCode;
        entity.PurchaseReturnGLCode = dto.PurchaseReturnGLCode;
        entity.SalesGLCode = dto.SalesGLCode;
        entity.SalesReturnGLCode = dto.SalesReturnGLCode;

        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
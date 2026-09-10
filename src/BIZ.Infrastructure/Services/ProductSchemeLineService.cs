using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductSchemeLineService : IProductSchemeLineService
{
    private readonly TenantDbContext _context;

    public ProductSchemeLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductSchemeLineDto>> GetAllAsync()
    {
        return await _context.ProductSchemeLines
            .AsNoTracking()
            .Where(x => x.ProductScheme.IsActive)
            .OrderBy(x => x.ProductSchemeId)
            .ThenBy(x => x.LineNumber)
            .Select(x => MapExpression(x))
            .ToListAsync();
    }

    public async Task<ProductSchemeLineDto?> GetByIdAsync(int id)
    {
        return await _context.ProductSchemeLines
            .AsNoTracking()
            .Where(x => x.Id == id && x.ProductScheme.IsActive)
            .Select(x => MapExpression(x))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ProductSchemeLineDto>> GetBySchemeAsync(
        int productSchemeId)
    {
        return await _context.ProductSchemeLines
            .AsNoTracking()
            .Where(x =>
                x.ProductSchemeId == productSchemeId &&
                x.ProductScheme.IsActive)
            .OrderBy(x => x.LineNumber)
            .Select(x => MapExpression(x))
            .ToListAsync();
    }

    public async Task<ProductSchemeLineDto> CreateAsync(
        ProductSchemeLineDto dto)
    {
        var scheme = await _context.ProductSchemes
            .FirstOrDefaultAsync(x => x.Id == dto.ProductSchemeId && x.IsActive);

        if (scheme == null)
            throw new InvalidOperationException(
                "Product scheme not found or inactive.");

        var productExists = await _context.Products
            .AnyAsync(x => x.Id == dto.ProductId && x.IsActive);

        if (!productExists)
            throw new InvalidOperationException(
                "Product not found or inactive.");

        if (dto.MinimumQuantity <= 0)
            throw new InvalidOperationException(
                "MinimumQuantity must be greater than zero.");

        if (dto.DiscountPercent < 0 || dto.DiscountPercent > 100)
            throw new InvalidOperationException(
                "DiscountPercent must be between 0 and 100.");

        var duplicateLine = await _context.ProductSchemeLines
            .AnyAsync(x =>
                x.ProductSchemeId == dto.ProductSchemeId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new InvalidOperationException(
                "LineNumber already exists for this scheme.");

        var entity = new ProductSchemeLine
        {
            ProductSchemeId = dto.ProductSchemeId,
            ProductId = dto.ProductId,
            MinimumQuantity = dto.MinimumQuantity,
            MaximumQuantity = dto.MaximumQuantity,
            DiscountPercent = dto.DiscountPercent,
            DiscountAmount = dto.DiscountAmount,
            FreeQuantity = dto.FreeQuantity,
            Description = dto.Description?.Trim(),
            LineNumber = dto.LineNumber
        };

        _context.ProductSchemeLines.Add(entity);
        await _context.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductSchemeLineDto dto)
    {
        var entity = await _context.ProductSchemeLines
            .Include(x => x.ProductScheme)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ProductScheme.IsActive);

        if (entity == null)
            return false;

        if (entity.ProductSchemeId != dto.ProductSchemeId)
            throw new InvalidOperationException(
                "ProductSchemeId cannot be changed.");

        var productExists = await _context.Products
            .AnyAsync(x => x.Id == dto.ProductId && x.IsActive);

        if (!productExists)
            throw new InvalidOperationException(
                "Product not found or inactive.");

        var duplicateLine = await _context.ProductSchemeLines
            .AnyAsync(x =>
                x.Id != id &&
                x.ProductSchemeId == dto.ProductSchemeId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new InvalidOperationException(
                "LineNumber already exists for this scheme.");

        entity.ProductId = dto.ProductId;
        entity.MinimumQuantity = dto.MinimumQuantity;
        entity.MaximumQuantity = dto.MaximumQuantity;
        entity.DiscountPercent = dto.DiscountPercent;
        entity.DiscountAmount = dto.DiscountAmount;
        entity.FreeQuantity = dto.FreeQuantity;
        entity.Description = dto.Description?.Trim();
        entity.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.ProductSchemeLines
            .Include(x => x.ProductScheme)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ProductScheme.IsActive);

        if (entity == null)
            return false;

        _context.ProductSchemeLines.Remove(entity);

        await _context.SaveChangesAsync();

        return true;
    }

    private static ProductSchemeLineDto Map(ProductSchemeLine x)
    {
        return new ProductSchemeLineDto
        {
            Id = x.Id,
            ProductSchemeId = x.ProductSchemeId,
            ProductId = x.ProductId,
            MinimumQuantity = x.MinimumQuantity,
            MaximumQuantity = x.MaximumQuantity,
            DiscountPercent = x.DiscountPercent,
            DiscountAmount = x.DiscountAmount,
            FreeQuantity = x.FreeQuantity,
            Description = x.Description,
            LineNumber = x.LineNumber
        };
    }

    private static ProductSchemeLineDto MapExpression(ProductSchemeLine x)
    {
        return Map(x);
    }
}
using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductCompositionLineService : IProductCompositionLineService
{
    private readonly TenantDbContext _context;

    public ProductCompositionLineService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductCompositionLineDto>> GetAllAsync()
    {
        return await _context.ProductCompositionLines
            .AsNoTracking()
            .Where(x => x.ProductComposition.IsActive)
            .OrderBy(x => x.ProductCompositionId)
            .ThenBy(x => x.LineNumber)
            .Select(x => new ProductCompositionLineDto
            {
                Id = x.Id,
                ProductCompositionId = x.ProductCompositionId,
                ComponentProductId = x.ComponentProductId,
                ComponentName = x.ComponentName,
                Quantity = x.Quantity,
                Unit = x.Unit,
                Percentage = x.Percentage,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<ProductCompositionLineDto?> GetByIdAsync(int id)
    {
        return await _context.ProductCompositionLines
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.ProductComposition.IsActive)
            .Select(x => new ProductCompositionLineDto
            {
                Id = x.Id,
                ProductCompositionId = x.ProductCompositionId,
                ComponentProductId = x.ComponentProductId,
                ComponentName = x.ComponentName,
                Quantity = x.Quantity,
                Unit = x.Unit,
                Percentage = x.Percentage,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ProductCompositionLineDto>>
        GetByCompositionAsync(int productCompositionId)
    {
        return await _context.ProductCompositionLines
            .AsNoTracking()
            .Where(x =>
                x.ProductCompositionId == productCompositionId &&
                x.ProductComposition.IsActive)
            .OrderBy(x => x.LineNumber)
            .Select(x => new ProductCompositionLineDto
            {
                Id = x.Id,
                ProductCompositionId = x.ProductCompositionId,
                ComponentProductId = x.ComponentProductId,
                ComponentName = x.ComponentName,
                Quantity = x.Quantity,
                Unit = x.Unit,
                Percentage = x.Percentage,
                Description = x.Description,
                LineNumber = x.LineNumber
            })
            .ToListAsync();
    }

    public async Task<ProductCompositionLineDto> CreateAsync(
        ProductCompositionLineDto dto)
    {
        var composition = await _context.ProductCompositions
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductCompositionId &&
                x.IsActive);

        if (composition == null)
            throw new InvalidOperationException(
                "Product composition not found or inactive.");

        if (string.IsNullOrWhiteSpace(dto.ComponentName))
            throw new InvalidOperationException(
                "ComponentName is required.");

        if (dto.Quantity < 0)
            throw new InvalidOperationException(
                "Quantity cannot be negative.");

        if (dto.Percentage < 0 || dto.Percentage > 100)
            throw new InvalidOperationException(
                "Percentage must be between 0 and 100.");

        if (dto.ComponentProductId.HasValue)
        {
            var productExists = await _context.Products
                .AnyAsync(x =>
                    x.Id == dto.ComponentProductId.Value &&
                    x.IsActive);

            if (!productExists)
                throw new InvalidOperationException(
                    "Component product not found or inactive.");
        }

        var duplicateLine = await _context.ProductCompositionLines
            .AnyAsync(x =>
                x.ProductCompositionId == dto.ProductCompositionId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new InvalidOperationException(
                "LineNumber already exists.");

        var currentPercentage =
            await _context.ProductCompositionLines
                .Where(x => x.ProductCompositionId == dto.ProductCompositionId)
                .SumAsync(x => x.Percentage);

        if (currentPercentage + dto.Percentage > 100)
            throw new InvalidOperationException(
                "Total composition percentage cannot exceed 100.");

        var entity = new ProductCompositionLine
        {
            ProductCompositionId = dto.ProductCompositionId,
            ComponentProductId = dto.ComponentProductId,
            ComponentName = dto.ComponentName.Trim(),
            Quantity = dto.Quantity,
            Unit = dto.Unit?.Trim(),
            Percentage = dto.Percentage,
            Description = dto.Description?.Trim(),
            LineNumber = dto.LineNumber
        };

        _context.ProductCompositionLines.Add(entity);

        await _context.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductCompositionLineDto dto)
    {
        var entity = await _context.ProductCompositionLines
            .Include(x => x.ProductComposition)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ProductComposition.IsActive);

        if (entity == null)
            return false;

        if (entity.ProductCompositionId != dto.ProductCompositionId)
            throw new InvalidOperationException(
                "ProductCompositionId cannot be changed.");

        var duplicateLine = await _context.ProductCompositionLines
            .AnyAsync(x =>
                x.Id != id &&
                x.ProductCompositionId == dto.ProductCompositionId &&
                x.LineNumber == dto.LineNumber);

        if (duplicateLine)
            throw new InvalidOperationException(
                "LineNumber already exists.");

        if (dto.ComponentProductId.HasValue)
        {
            var productExists = await _context.Products
                .AnyAsync(x =>
                    x.Id == dto.ComponentProductId.Value &&
                    x.IsActive);

            if (!productExists)
                throw new InvalidOperationException(
                    "Component product not found or inactive.");
        }

        var otherPercentage =
            await _context.ProductCompositionLines
                .Where(x =>
                    x.ProductCompositionId == dto.ProductCompositionId &&
                    x.Id != id)
                .SumAsync(x => x.Percentage);

        if (otherPercentage + dto.Percentage > 100)
            throw new InvalidOperationException(
                "Total composition percentage cannot exceed 100.");

        entity.ComponentProductId = dto.ComponentProductId;
        entity.ComponentName = dto.ComponentName.Trim();
        entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit?.Trim();
        entity.Percentage = dto.Percentage;
        entity.Description = dto.Description?.Trim();
        entity.LineNumber = dto.LineNumber;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.ProductCompositionLines
            .Include(x => x.ProductComposition)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ProductComposition.IsActive);

        if (entity == null)
            return false;

        _context.ProductCompositionLines.Remove(entity);

        await _context.SaveChangesAsync();

        return true;
    }

    private static ProductCompositionLineDto Map(
        ProductCompositionLine x)
    {
        return new ProductCompositionLineDto
        {
            Id = x.Id,
            ProductCompositionId = x.ProductCompositionId,
            ComponentProductId = x.ComponentProductId,
            ComponentName = x.ComponentName,
            Quantity = x.Quantity,
            Unit = x.Unit,
            Percentage = x.Percentage,
            Description = x.Description,
            LineNumber = x.LineNumber
        };
    }
}
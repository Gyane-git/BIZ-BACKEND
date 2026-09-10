using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductCompositionService : IProductCompositionService
{
    private readonly TenantDbContext _context;

    public ProductCompositionService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductCompositionDto>> GetAllAsync()
    {
        return await _context.ProductCompositions
            .AsNoTracking()
            .Include(x => x.ProductCompositionLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new ProductCompositionDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                CompositionName = x.CompositionName,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Lines = x.ProductCompositionLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new ProductCompositionLineDto
                    {
                        Id = l.Id,
                        ProductCompositionId = l.ProductCompositionId,
                        ComponentProductId = l.ComponentProductId,
                        ComponentName = l.ComponentName,
                        Quantity = l.Quantity,
                        Unit = l.Unit,
                        Percentage = l.Percentage,
                        Description = l.Description,
                        LineNumber = l.LineNumber
                    }).ToList()
            })
            .ToListAsync();
    }

    public async Task<ProductCompositionDto?> GetByIdAsync(int id)
    {
        var entity = await _context.ProductCompositions
            .AsNoTracking()
            .Include(x => x.ProductCompositionLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        return entity == null ? null : Map(entity);
    }

    public async Task<ProductCompositionDto> CreateAsync(
        ProductCompositionDto dto)
    {
        var productExists = await _context.Products
            .AnyAsync(x => x.Id == dto.ProductId && x.IsActive);

        if (!productExists)
            throw new InvalidOperationException(
                "Product not found or inactive.");

        if (string.IsNullOrWhiteSpace(dto.CompositionName))
            throw new InvalidOperationException(
                "CompositionName is required.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one composition line is required.");

        var duplicate = await _context.ProductCompositions
            .AnyAsync(x =>
                x.ProductId == dto.ProductId &&
                x.CompositionName == dto.CompositionName.Trim());

        if (duplicate)
            throw new InvalidOperationException(
                "This composition already exists for the product.");

        var totalPercentage = dto.Lines.Sum(x => x.Percentage);

        if (totalPercentage > 100)
            throw new InvalidOperationException(
                "Total composition percentage cannot exceed 100.");

        var entity = new ProductComposition
        {
            ProductId = dto.ProductId,
            CompositionName = dto.CompositionName.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var line in dto.Lines.OrderBy(x => x.LineNumber))
        {
            if (string.IsNullOrWhiteSpace(line.ComponentName))
                throw new InvalidOperationException(
                    "ComponentName is required.");

            if (line.Quantity < 0)
                throw new InvalidOperationException(
                    "Quantity cannot be negative.");

            if (line.Percentage < 0 || line.Percentage > 100)
                throw new InvalidOperationException(
                    "Percentage must be between 0 and 100.");

            if (line.ComponentProductId.HasValue)
            {
                var componentExists = await _context.Products
                    .AnyAsync(x =>
                        x.Id == line.ComponentProductId.Value &&
                        x.IsActive);

                if (!componentExists)
                    throw new InvalidOperationException(
                        $"ComponentProductId {line.ComponentProductId} is invalid.");
            }

            entity.ProductCompositionLines.Add(
                new ProductCompositionLine
                {
                    ComponentProductId = line.ComponentProductId,
                    ComponentName = line.ComponentName.Trim(),
                    Quantity = line.Quantity,
                    Unit = line.Unit?.Trim(),
                    Percentage = line.Percentage,
                    Description = line.Description?.Trim(),
                    LineNumber = line.LineNumber
                });
        }

        _context.ProductCompositions.Add(entity);

        await _context.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<bool> UpdateAsync(
        int id,
        ProductCompositionDto dto)
    {
        var entity = await _context.ProductCompositions
            .Include(x => x.ProductCompositionLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return false;

        var productExists = await _context.Products
            .AnyAsync(x => x.Id == dto.ProductId && x.IsActive);

        if (!productExists)
            throw new InvalidOperationException(
                "Product not found or inactive.");

        var duplicate = await _context.ProductCompositions
            .AnyAsync(x =>
                x.Id != id &&
                x.ProductId == dto.ProductId &&
                x.CompositionName == dto.CompositionName.Trim());

        if (duplicate)
            throw new InvalidOperationException(
                "This composition already exists for the product.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one composition line is required.");

        var totalPercentage = dto.Lines.Sum(x => x.Percentage);

        if (totalPercentage > 100)
            throw new InvalidOperationException(
                "Total composition percentage cannot exceed 100.");

        foreach (var oldLine in entity.ProductCompositionLines.ToList())
            _context.ProductCompositionLines.Remove(oldLine);

        entity.ProductId = dto.ProductId;
        entity.CompositionName = dto.CompositionName.Trim();
        entity.Description = dto.Description?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        foreach (var line in dto.Lines.OrderBy(x => x.LineNumber))
        {
            entity.ProductCompositionLines.Add(
                new ProductCompositionLine
                {
                    ProductCompositionId = entity.Id,
                    ComponentProductId = line.ComponentProductId,
                    ComponentName = line.ComponentName.Trim(),
                    Quantity = line.Quantity,
                    Unit = line.Unit?.Trim(),
                    Percentage = line.Percentage,
                    Description = line.Description?.Trim(),
                    LineNumber = line.LineNumber
                });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.ProductCompositions
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static ProductCompositionDto Map(ProductComposition x)
    {
        return new ProductCompositionDto
        {
            Id = x.Id,
            ProductId = x.ProductId,
            CompositionName = x.CompositionName,
            Description = x.Description,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            Lines = x.ProductCompositionLines
                .OrderBy(l => l.LineNumber)
                .Select(l => new ProductCompositionLineDto
                {
                    Id = l.Id,
                    ProductCompositionId = l.ProductCompositionId,
                    ComponentProductId = l.ComponentProductId,
                    ComponentName = l.ComponentName,
                    Quantity = l.Quantity,
                    Unit = l.Unit,
                    Percentage = l.Percentage,
                    Description = l.Description,
                    LineNumber = l.LineNumber
                })
                .ToList()
        };
    }
}
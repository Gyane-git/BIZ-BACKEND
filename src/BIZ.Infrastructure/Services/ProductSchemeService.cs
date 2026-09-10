using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ProductSchemeService : IProductSchemeService
{
    private readonly TenantDbContext _context;

    public ProductSchemeService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductSchemeDto>> GetAllAsync()
    {
        return await _context.ProductSchemes
            .AsNoTracking()
            .Include(x => x.ProductSchemeLines)
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => new ProductSchemeDto
            {
                Id = x.Id,
                SchemeCode = x.SchemeCode,
                SchemeName = x.SchemeName,
                SchemeType = x.SchemeType,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Lines = x.ProductSchemeLines
                    .OrderBy(l => l.LineNumber)
                    .Select(l => new ProductSchemeLineDto
                    {
                        Id = l.Id,
                        ProductSchemeId = l.ProductSchemeId,
                        ProductId = l.ProductId,
                        MinimumQuantity = l.MinimumQuantity,
                        MaximumQuantity = l.MaximumQuantity,
                        DiscountPercent = l.DiscountPercent,
                        DiscountAmount = l.DiscountAmount,
                        FreeQuantity = l.FreeQuantity,
                        Description = l.Description,
                        LineNumber = l.LineNumber
                    }).ToList()
            })
            .ToListAsync();
    }

    public async Task<ProductSchemeDto?> GetByIdAsync(int id)
    {
        var entity = await _context.ProductSchemes
            .AsNoTracking()
            .Include(x => x.ProductSchemeLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return null;

        return MapToDto(entity);
    }

    public async Task<ProductSchemeDto> CreateAsync(ProductSchemeDto dto)
    {
        var code = dto.SchemeCode.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("SchemeCode is required.");

        if (string.IsNullOrWhiteSpace(dto.SchemeName))
            throw new InvalidOperationException("SchemeName is required.");

        if (dto.FromDate == default)
            throw new InvalidOperationException("FromDate is required.");

        if (dto.ToDate.HasValue && dto.ToDate.Value < dto.FromDate)
            throw new InvalidOperationException("ToDate cannot be earlier than FromDate.");

        var exists = await _context.ProductSchemes
            .AnyAsync(x => x.SchemeCode == code);

        if (exists)
            throw new InvalidOperationException(
                $"Product scheme '{code}' already exists.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one scheme line is required.");

        var lineNumbers = dto.Lines.Select(x => x.LineNumber).ToList();

        if (lineNumbers.Count != lineNumbers.Distinct().Count())
            throw new InvalidOperationException(
                "Duplicate LineNumber is not allowed.");

        var productIds = dto.Lines.Select(x => x.ProductId).Distinct().ToList();

        var validProducts = await _context.Products
            .Where(x => productIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        if (validProducts.Count != productIds.Count)
            throw new InvalidOperationException(
                "One or more ProductId values are invalid or inactive.");

        foreach (var line in dto.Lines)
        {
            if (line.MinimumQuantity <= 0)
                throw new InvalidOperationException(
                    "MinimumQuantity must be greater than zero.");

            if (line.MaximumQuantity.HasValue &&
                line.MaximumQuantity.Value < line.MinimumQuantity)
                throw new InvalidOperationException(
                    "MaximumQuantity cannot be less than MinimumQuantity.");

            if (line.DiscountPercent < 0 || line.DiscountPercent > 100)
                throw new InvalidOperationException(
                    "DiscountPercent must be between 0 and 100.");

            if (line.DiscountAmount < 0)
                throw new InvalidOperationException(
                    "DiscountAmount cannot be negative.");

            if (line.FreeQuantity < 0)
                throw new InvalidOperationException(
                    "FreeQuantity cannot be negative.");
        }

        var entity = new ProductScheme
        {
            SchemeCode = code,
            SchemeName = dto.SchemeName.Trim(),
            SchemeType = string.IsNullOrWhiteSpace(dto.SchemeType)
                ? "Quantity"
                : dto.SchemeType.Trim(),
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            Description = dto.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var line in dto.Lines.OrderBy(x => x.LineNumber))
        {
            entity.ProductSchemeLines.Add(new ProductSchemeLine
            {
                ProductId = line.ProductId,
                MinimumQuantity = line.MinimumQuantity,
                MaximumQuantity = line.MaximumQuantity,
                DiscountPercent = line.DiscountPercent,
                DiscountAmount = line.DiscountAmount,
                FreeQuantity = line.FreeQuantity,
                Description = line.Description?.Trim(),
                LineNumber = line.LineNumber
            });
        }

        _context.ProductSchemes.Add(entity);
        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, ProductSchemeDto dto)
    {
        var entity = await _context.ProductSchemes
            .Include(x => x.ProductSchemeLines)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return false;

        var code = dto.SchemeCode.Trim().ToUpperInvariant();

        var duplicate = await _context.ProductSchemes
            .AnyAsync(x => x.Id != id && x.SchemeCode == code);

        if (duplicate)
            throw new InvalidOperationException(
                $"Product scheme '{code}' already exists.");

        if (dto.Lines == null || dto.Lines.Count == 0)
            throw new InvalidOperationException(
                "At least one scheme line is required.");

        var productIds = dto.Lines
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var validProducts = await _context.Products
            .Where(x => productIds.Contains(x.Id) && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync();

        if (validProducts.Count != productIds.Count)
            throw new InvalidOperationException(
                "One or more ProductId values are invalid or inactive.");

        foreach (var oldLine in entity.ProductSchemeLines.ToList())
            _context.ProductSchemeLines.Remove(oldLine);

        entity.SchemeCode = code;
        entity.SchemeName = dto.SchemeName.Trim();
        entity.SchemeType = string.IsNullOrWhiteSpace(dto.SchemeType)
            ? "Quantity"
            : dto.SchemeType.Trim();
        entity.FromDate = dto.FromDate;
        entity.ToDate = dto.ToDate;
        entity.Description = dto.Description?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        foreach (var line in dto.Lines.OrderBy(x => x.LineNumber))
        {
            entity.ProductSchemeLines.Add(new ProductSchemeLine
            {
                ProductSchemeId = entity.Id,
                ProductId = line.ProductId,
                MinimumQuantity = line.MinimumQuantity,
                MaximumQuantity = line.MaximumQuantity,
                DiscountPercent = line.DiscountPercent,
                DiscountAmount = line.DiscountAmount,
                FreeQuantity = line.FreeQuantity,
                Description = line.Description?.Trim(),
                LineNumber = line.LineNumber
            });
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.ProductSchemes
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static ProductSchemeDto MapToDto(ProductScheme entity)
    {
        return new ProductSchemeDto
        {
            Id = entity.Id,
            SchemeCode = entity.SchemeCode,
            SchemeName = entity.SchemeName,
            SchemeType = entity.SchemeType,
            FromDate = entity.FromDate,
            ToDate = entity.ToDate,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Lines = entity.ProductSchemeLines
                .OrderBy(x => x.LineNumber)
                .Select(x => new ProductSchemeLineDto
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
                })
                .ToList()
        };
    }
}
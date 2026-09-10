using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class ValueAddedListService : IValueAddedListService
{
    private readonly TenantDbContext _context;

    public ValueAddedListService(TenantDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ValueAddedListDto>> GetAllAsync()
    {
        return await _context.ValueAddedLists
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ValueAddedListDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                DefaultAmount = x.DefaultAmount,
                AmountType = x.AmountType,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ValueAddedListDto?> GetByIdAsync(int id)
    {
        return await _context.ValueAddedLists
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new ValueAddedListDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                DefaultAmount = x.DefaultAmount,
                AmountType = x.AmountType,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ValueAddedListDto> CreateAsync(
        ValueAddedListDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Code is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Name is required.");

        if (dto.DefaultAmount < 0)
            throw new InvalidOperationException(
                "DefaultAmount cannot be negative.");

        var amountType = string.IsNullOrWhiteSpace(dto.AmountType)
            ? "Fixed"
            : dto.AmountType.Trim();

        var allowedTypes = new[] { "Fixed", "Percentage" };

        if (!allowedTypes.Contains(
                amountType,
                StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "AmountType must be Fixed or Percentage.");

        var exists = await _context.ValueAddedLists
            .AnyAsync(x => x.Code == code);

        if (exists)
            throw new InvalidOperationException(
                $"Value added code '{code}' already exists.");

        if (amountType.Equals(
                "Percentage",
                StringComparison.OrdinalIgnoreCase) &&
            dto.DefaultAmount > 100)
            throw new InvalidOperationException(
                "Percentage cannot exceed 100.");

        var entity = new ValueAddedList
        {
            Code = code,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            DefaultAmount = dto.DefaultAmount,
            AmountType = amountType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ValueAddedLists.Add(entity);

        await _context.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<bool> UpdateAsync(
        int id,
        ValueAddedListDto dto)
    {
        var entity = await _context.ValueAddedLists
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return false;

        var code = dto.Code.Trim().ToUpperInvariant();

        var duplicate = await _context.ValueAddedLists
            .AnyAsync(x => x.Id != id && x.Code == code);

        if (duplicate)
            throw new InvalidOperationException(
                $"Value added code '{code}' already exists.");

        var amountType = string.IsNullOrWhiteSpace(dto.AmountType)
            ? "Fixed"
            : dto.AmountType.Trim();

        if (dto.DefaultAmount < 0)
            throw new InvalidOperationException(
                "DefaultAmount cannot be negative.");

        if (amountType.Equals(
                "Percentage",
                StringComparison.OrdinalIgnoreCase) &&
            dto.DefaultAmount > 100)
            throw new InvalidOperationException(
                "Percentage cannot exceed 100.");

        entity.Code = code;
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description?.Trim();
        entity.DefaultAmount = dto.DefaultAmount;
        entity.AmountType = amountType;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.ValueAddedLists
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static ValueAddedListDto Map(ValueAddedList x)
    {
        return new ValueAddedListDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            DefaultAmount = x.DefaultAmount,
            AmountType = x.AmountType,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }
}
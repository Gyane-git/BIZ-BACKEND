using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly TenantDbContext _db;

    public CurrencyService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<CurrencyDto>> GetAllAsync()
    {
        return await _db.Currencies
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new CurrencyDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Symbol = x.Symbol,
                Description = x.Description,
                IsBaseCurrency = x.IsBaseCurrency,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<CurrencyDto?> GetByIdAsync(int id)
    {
        return await _db.Currencies
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CurrencyDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Symbol = x.Symbol,
                Description = x.Description,
                IsBaseCurrency = x.IsBaseCurrency,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CurrencyDto> CreateAsync(
        CurrencyDto dto)
    {
        var code = dto.Code.Trim().ToUpper();

        var exists = await _db.Currencies
            .AnyAsync(x => x.Code == code);

        if (exists)
            throw new InvalidOperationException(
                "Currency code already exists.");

        if (dto.IsBaseCurrency)
        {
            var baseExists = await _db.Currencies
                .AnyAsync(x => x.IsBaseCurrency);

            if (baseExists)
                throw new InvalidOperationException(
                    "Base currency already exists.");
        }

        var entity = new Currency
        {
            Code = code,
            Name = dto.Name.Trim(),
            Symbol = dto.Symbol?.Trim(),
            Description = dto.Description?.Trim(),
            IsBaseCurrency = dto.IsBaseCurrency,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Currencies.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.Code = entity.Code;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        CurrencyDto dto)
    {
        var entity = await _db.Currencies
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var code = dto.Code.Trim().ToUpper();

        var exists = await _db.Currencies
            .AnyAsync(x =>
                x.Code == code &&
                x.Id != id);

        if (exists)
            throw new InvalidOperationException(
                "Currency code already exists.");

        if (dto.IsBaseCurrency &&
            !entity.IsBaseCurrency)
        {
            var baseExists = await _db.Currencies
                .AnyAsync(x =>
                    x.IsBaseCurrency &&
                    x.Id != id);

            if (baseExists)
                throw new InvalidOperationException(
                    "Another base currency already exists.");
        }

        entity.Code = code;
        entity.Name = dto.Name.Trim();
        entity.Symbol = dto.Symbol?.Trim();
        entity.Description = dto.Description?.Trim();
        entity.IsBaseCurrency = dto.IsBaseCurrency;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.Currencies
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        if (entity.IsBaseCurrency)
            throw new InvalidOperationException(
                "Base currency cannot be deleted.");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}
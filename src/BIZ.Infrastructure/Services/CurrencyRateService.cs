using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class CurrencyRateService : ICurrencyRateService
{
    private readonly TenantDbContext _db;

    public CurrencyRateService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<CurrencyRateDto>> GetAllAsync()
    {
        return await _db.CurrencyRates
            .AsNoTracking()
            .OrderByDescending(x => x.RateDate)
            .Select(x => new CurrencyRateDto
            {
                Id = x.Id,
                CurrencyId = x.CurrencyId,
                RateDate = x.RateDate,
                BuyingRate = x.BuyingRate,
                SellingRate = x.SellingRate,
                AverageRate = x.AverageRate,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<CurrencyRateDto>>
        GetByCurrencyAsync(int currencyId)
    {
        return await _db.CurrencyRates
            .AsNoTracking()
            .Where(x => x.CurrencyId == currencyId)
            .OrderByDescending(x => x.RateDate)
            .Select(x => new CurrencyRateDto
            {
                Id = x.Id,
                CurrencyId = x.CurrencyId,
                RateDate = x.RateDate,
                BuyingRate = x.BuyingRate,
                SellingRate = x.SellingRate,
                AverageRate = x.AverageRate,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<CurrencyRateDto?>
        GetByIdAsync(int id)
    {
        return await _db.CurrencyRates
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CurrencyRateDto
            {
                Id = x.Id,
                CurrencyId = x.CurrencyId,
                RateDate = x.RateDate,
                BuyingRate = x.BuyingRate,
                SellingRate = x.SellingRate,
                AverageRate = x.AverageRate,
                Remarks = x.Remarks,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CurrencyRateDto>
        CreateAsync(CurrencyRateDto dto)
    {
        var currencyExists = await _db.Currencies
            .AnyAsync(x =>
                x.Id == dto.CurrencyId &&
                x.IsActive);

        if (!currencyExists)
            throw new InvalidOperationException(
                "Currency not found or inactive.");

        if (dto.BuyingRate < 0 ||
            dto.SellingRate < 0)
        {
            throw new InvalidOperationException(
                "Currency rates cannot be negative.");
        }

        if (dto.SellingRate < dto.BuyingRate)
        {
            throw new InvalidOperationException(
                "Selling rate cannot be lower than buying rate.");
        }

        var exists = await _db.CurrencyRates
            .AnyAsync(x =>
                x.CurrencyId == dto.CurrencyId &&
                x.RateDate.Date == dto.RateDate.Date);

        if (exists)
            throw new InvalidOperationException(
                "Currency rate already exists for this date.");

        var entity = new CurrencyRate
        {
            CurrencyId = dto.CurrencyId,
            RateDate = dto.RateDate.Date,
            BuyingRate = dto.BuyingRate,
            SellingRate = dto.SellingRate,
            AverageRate = dto.AverageRate ??
                ((dto.BuyingRate + dto.SellingRate) / 2),
            Remarks = dto.Remarks?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.CurrencyRates.Add(entity);

        await _db.SaveChangesAsync();

        dto.Id = entity.Id;
        dto.RateDate = entity.RateDate;
        dto.AverageRate = entity.AverageRate;

        return dto;
    }

    public async Task<bool> UpdateAsync(
        int id,
        CurrencyRateDto dto)
    {
        var entity = await _db.CurrencyRates
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        var currencyExists = await _db.Currencies
            .AnyAsync(x =>
                x.Id == dto.CurrencyId &&
                x.IsActive);

        if (!currencyExists)
            throw new InvalidOperationException(
                "Currency not found or inactive.");

        if (dto.BuyingRate < 0 ||
            dto.SellingRate < 0)
        {
            throw new InvalidOperationException(
                "Currency rates cannot be negative.");
        }

        if (dto.SellingRate < dto.BuyingRate)
        {
            throw new InvalidOperationException(
                "Selling rate cannot be lower than buying rate.");
        }

        var exists = await _db.CurrencyRates
            .AnyAsync(x =>
                x.CurrencyId == dto.CurrencyId &&
                x.RateDate.Date == dto.RateDate.Date &&
                x.Id != id);

        if (exists)
            throw new InvalidOperationException(
                "Currency rate already exists for this date.");

        entity.CurrencyId = dto.CurrencyId;
        entity.RateDate = dto.RateDate.Date;
        entity.BuyingRate = dto.BuyingRate;
        entity.SellingRate = dto.SellingRate;
        entity.AverageRate = dto.AverageRate ??
            ((dto.BuyingRate + dto.SellingRate) / 2);
        entity.Remarks = dto.Remarks?.Trim();
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.CurrencyRates
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }
}

using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ICurrencyRateService
{
    Task<List<CurrencyRateDto>> GetAllAsync();

    Task<List<CurrencyRateDto>> GetByCurrencyAsync(
        int currencyId);

    Task<CurrencyRateDto?> GetByIdAsync(int id);

    Task<CurrencyRateDto> CreateAsync(
        CurrencyRateDto dto);

    Task<bool> UpdateAsync(
        int id,
        CurrencyRateDto dto);

    Task<bool> DeleteAsync(int id);
}
using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ICurrencyService
{
    Task<List<CurrencyDto>> GetAllAsync();

    Task<CurrencyDto?> GetByIdAsync(int id);

    Task<CurrencyDto> CreateAsync(CurrencyDto dto);

    Task<bool> UpdateAsync(int id, CurrencyDto dto);

    Task<bool> DeleteAsync(int id);
}
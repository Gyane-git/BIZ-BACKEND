using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockOpeningService
{
    Task<IEnumerable<StockOpeningDto>> GetAllAsync();

    Task<StockOpeningDto?> GetByIdAsync(int id);

    Task<StockOpeningDto> CreateAsync(StockOpeningDto dto);

    Task<bool> UpdateAsync(int id, StockOpeningDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> PostAsync(int id);
}
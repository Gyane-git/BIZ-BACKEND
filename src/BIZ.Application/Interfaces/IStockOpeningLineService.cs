using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockOpeningLineService
{
    Task<IEnumerable<StockOpeningLineDto>> GetAllAsync();

    Task<StockOpeningLineDto?> GetByIdAsync(int id);

    Task<IEnumerable<StockOpeningLineDto>> GetByOpeningAsync(int stockOpeningId);

    Task<StockOpeningLineDto> CreateAsync(StockOpeningLineDto dto);

    Task<bool> UpdateAsync(int id, StockOpeningLineDto dto);

    Task<bool> DeleteAsync(int id);
}
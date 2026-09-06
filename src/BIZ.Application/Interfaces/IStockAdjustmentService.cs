using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockAdjustmentService
{
    Task<IEnumerable<StockAdjustmentDto>> GetAllAsync();

    Task<StockAdjustmentDto?> GetByIdAsync(int id);

    Task<StockAdjustmentDto> CreateAsync(StockAdjustmentDto dto);

    Task<bool> UpdateAsync(int id, StockAdjustmentDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> PostAsync(int id);
}
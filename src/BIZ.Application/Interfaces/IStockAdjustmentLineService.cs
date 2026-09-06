using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockAdjustmentLineService
{
    Task<IEnumerable<StockAdjustmentLineDto>> GetAllAsync();

    Task<StockAdjustmentLineDto?> GetByIdAsync(int id);

    Task<IEnumerable<StockAdjustmentLineDto>> GetByAdjustmentAsync(
        int stockAdjustmentId);

    Task<StockAdjustmentLineDto> CreateAsync(
        StockAdjustmentLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        StockAdjustmentLineDto dto);

    Task<bool> DeleteAsync(int id);
}
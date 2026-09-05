using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockBalanceService
{
    Task<IEnumerable<StockBalanceDto>> GetAllAsync();

    Task<StockBalanceDto?> GetByIdAsync(int id);

    Task<IEnumerable<StockBalanceDto>> GetByProductAsync(
        int productId);

    Task<IEnumerable<StockBalanceDto>> GetByWarehouseAsync(
        int warehouseId);

    Task<StockBalanceDto> CreateAsync(
        StockBalanceDto dto);

    Task<bool> UpdateAsync(
        int id,
        StockBalanceDto dto);

    Task<bool> DeleteAsync(int id);
}
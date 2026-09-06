using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockTransferService
{
    Task<IEnumerable<StockTransferDto>> GetAllAsync();

    Task<StockTransferDto?> GetByIdAsync(int id);

    Task<StockTransferDto> CreateAsync(
        StockTransferDto dto);

    Task<bool> UpdateAsync(
        int id,
        StockTransferDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> PostAsync(int id);
}
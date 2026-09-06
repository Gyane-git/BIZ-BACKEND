using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockTransferLineService
{
    Task<IEnumerable<StockTransferLineDto>> GetAllAsync();

    Task<StockTransferLineDto?> GetByIdAsync(int id);

    Task<IEnumerable<StockTransferLineDto>> GetByTransferAsync(
        int stockTransferId);

    Task<StockTransferLineDto> CreateAsync(
        StockTransferLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        StockTransferLineDto dto);

    Task<bool> DeleteAsync(int id);
}
using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockTransactionService
{
    Task<IEnumerable<StockTransactionDto>> GetAllAsync();

    Task<StockTransactionDto?> GetByIdAsync(int id);

    Task<IEnumerable<StockTransactionDto>> GetByProductAsync(
        int productId);

    Task<StockTransactionDto> CreateAsync(
        StockTransactionDto dto);

    Task<bool> DeleteAsync(int id);
}
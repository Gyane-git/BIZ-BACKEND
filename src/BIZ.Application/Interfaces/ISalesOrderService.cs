using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderDto>> GetAllAsync();

    Task<SalesOrderDto?> GetByIdAsync(int id);

    Task<SalesOrderDto> CreateAsync(SalesOrderDto dto);

    Task<bool> UpdateAsync(int id, SalesOrderDto dto);

    Task<bool> DeleteAsync(int id);
}
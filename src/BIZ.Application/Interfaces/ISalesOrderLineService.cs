using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesOrderLineService
{
    Task<IEnumerable<SalesOrderLineDto>> GetAllAsync();

    Task<SalesOrderLineDto?> GetByIdAsync(int id);

    Task<SalesOrderLineDto> CreateAsync(
        SalesOrderLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        SalesOrderLineDto dto);

    Task<bool> DeleteAsync(int id);
}
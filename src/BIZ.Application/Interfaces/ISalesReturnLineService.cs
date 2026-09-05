using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesReturnLineService
{
    Task<IEnumerable<SalesReturnLineDto>> GetAllAsync();

    Task<SalesReturnLineDto?> GetByIdAsync(int id);

    Task<SalesReturnLineDto> CreateAsync(
        SalesReturnLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        SalesReturnLineDto dto);

    Task<bool> DeleteAsync(int id);
}
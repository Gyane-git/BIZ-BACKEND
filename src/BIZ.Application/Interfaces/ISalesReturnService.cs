using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesReturnService
{
    Task<IEnumerable<SalesReturnDto>> GetAllAsync();

    Task<SalesReturnDto?> GetByIdAsync(int id);

    Task<SalesReturnDto> CreateAsync(SalesReturnDto dto);

    Task<bool> UpdateAsync(int id, SalesReturnDto dto);

    Task<bool> PostAsync(int id);

    Task<bool> DeleteAsync(int id);
}

using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesPaymentService
{
    Task<IEnumerable<SalesPaymentDto>> GetAllAsync();

    Task<SalesPaymentDto?> GetByIdAsync(int id);

    Task<SalesPaymentDto> CreateAsync(SalesPaymentDto dto);

    Task<bool> UpdateAsync(int id, SalesPaymentDto dto);

    Task<bool> DeleteAsync(int id);
}
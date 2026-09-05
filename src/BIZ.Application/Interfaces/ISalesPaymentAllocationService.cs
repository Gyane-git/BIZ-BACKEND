using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesPaymentAllocationService
{
    Task<IEnumerable<SalesPaymentAllocationDto>> GetAllAsync();

    Task<SalesPaymentAllocationDto?> GetByIdAsync(int id);

    Task<SalesPaymentAllocationDto> CreateAsync(
        SalesPaymentAllocationDto dto);

    Task<bool> UpdateAsync(
        int id,
        SalesPaymentAllocationDto dto);

    Task<bool> DeleteAsync(int id);
}
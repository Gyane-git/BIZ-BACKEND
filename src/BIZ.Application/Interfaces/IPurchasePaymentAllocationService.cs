using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchasePaymentAllocationService
{
    Task<IEnumerable<PurchasePaymentAllocationDto>> GetAllAsync();

    Task<PurchasePaymentAllocationDto?> GetByIdAsync(int id);

    Task<PurchasePaymentAllocationDto> CreateAsync(
        PurchasePaymentAllocationDto dto);

    Task<bool> UpdateAsync(
        int id,
        PurchasePaymentAllocationDto dto);

    Task<bool> DeleteAsync(int id);
}
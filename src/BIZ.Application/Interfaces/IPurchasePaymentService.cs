using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchasePaymentService
{
    Task<IEnumerable<PurchasePaymentDto>> GetAllAsync();

    Task<PurchasePaymentDto?> GetByIdAsync(int id);

    Task<PurchasePaymentDto> CreateAsync(PurchasePaymentDto dto);

    Task<bool> UpdateAsync(int id, PurchasePaymentDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> PostAsync(int id);
}
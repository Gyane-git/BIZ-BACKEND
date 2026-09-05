using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseReturnService
{
    Task<IEnumerable<PurchaseReturnDto>> GetAllAsync();

    Task<PurchaseReturnDto?> GetByIdAsync(int id);

    Task<PurchaseReturnDto> CreateAsync(
        PurchaseReturnDto dto);

    Task<bool> UpdateAsync(
        int id,
        PurchaseReturnDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> PostAsync(int id);
}
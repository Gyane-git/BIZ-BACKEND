using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseReturnLineService
{
    Task<IEnumerable<PurchaseReturnLineDto>> GetAllAsync();

    Task<PurchaseReturnLineDto?> GetByIdAsync(int id);

    Task<PurchaseReturnLineDto> CreateAsync(
        PurchaseReturnLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        PurchaseReturnLineDto dto);

    Task<bool> DeleteAsync(int id);
}
using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseRequestService
{
    Task<IEnumerable<PurchaseRequestDto>> GetAllAsync();

    Task<PurchaseRequestDto?> GetByIdAsync(int id);

    Task<PurchaseRequestDto> CreateAsync(PurchaseRequestDto dto);

    Task<bool> UpdateAsync(int id, PurchaseRequestDto dto);

    Task<bool> DeleteAsync(int id);
}
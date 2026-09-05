using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseRequestLineService
{
    Task<IEnumerable<PurchaseRequestLineDto>> GetAllAsync();

    Task<PurchaseRequestLineDto?> GetByIdAsync(int id);

    Task<PurchaseRequestLineDto> CreateAsync(
        PurchaseRequestLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        PurchaseRequestLineDto dto);

    Task<bool> DeleteAsync(int id);
}
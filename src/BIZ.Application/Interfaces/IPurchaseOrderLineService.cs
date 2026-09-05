using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseOrderLineService
{
    Task<IEnumerable<PurchaseOrderLineDto>> GetAllAsync();

    Task<PurchaseOrderLineDto?> GetByIdAsync(int id);

    Task<PurchaseOrderLineDto> CreateAsync(
        PurchaseOrderLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        PurchaseOrderLineDto dto);

    Task<bool> DeleteAsync(int id);
}
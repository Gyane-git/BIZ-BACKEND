using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<IEnumerable<PurchaseOrderDto>> GetAllAsync();

    Task<PurchaseOrderDto?> GetByIdAsync(int id);

    Task<PurchaseOrderDto> CreateAsync(PurchaseOrderDto dto);

    Task<bool> UpdateAsync(int id, PurchaseOrderDto dto);

    Task<bool> DeleteAsync(int id);
}
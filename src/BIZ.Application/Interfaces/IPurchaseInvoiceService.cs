using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseInvoiceService
{
    Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync();

    Task<PurchaseInvoiceDto?> GetByIdAsync(int id);

    Task<PurchaseInvoiceDto> CreateAsync(PurchaseInvoiceDto dto);

    Task<bool> UpdateAsync(int id, PurchaseInvoiceDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> PostAsync(int id);
}
using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseInvoiceLineService
{
    Task<IEnumerable<PurchaseInvoiceLineDto>> GetAllAsync();

    Task<PurchaseInvoiceLineDto?> GetByIdAsync(int id);

    Task<PurchaseInvoiceLineDto> CreateAsync(
        PurchaseInvoiceLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        PurchaseInvoiceLineDto dto);

    Task<bool> DeleteAsync(int id);
}
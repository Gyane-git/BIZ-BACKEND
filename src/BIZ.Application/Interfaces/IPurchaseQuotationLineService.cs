using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseQuotationLineService
{
    Task<IEnumerable<PurchaseQuotationLineDto>> GetAllAsync();

    Task<PurchaseQuotationLineDto?> GetByIdAsync(int id);

    Task<PurchaseQuotationLineDto> CreateAsync(PurchaseQuotationLineDto dto);

    Task<bool> UpdateAsync(int id, PurchaseQuotationLineDto dto);

    Task<bool> DeleteAsync(int id);
}
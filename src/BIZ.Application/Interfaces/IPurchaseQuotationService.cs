using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPurchaseQuotationService
{
    Task<IEnumerable<PurchaseQuotationDto>> GetAllAsync();

    Task<PurchaseQuotationDto?> GetByIdAsync(int id);

    Task<PurchaseQuotationDto> CreateAsync(PurchaseQuotationDto dto);

    Task<bool> UpdateAsync(int id, PurchaseQuotationDto dto);

    Task<bool> DeleteAsync(int id);
}
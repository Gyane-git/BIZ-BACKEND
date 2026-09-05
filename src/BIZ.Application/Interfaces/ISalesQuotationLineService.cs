using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesQuotationLineService
{
    Task<IEnumerable<SalesQuotationLineDto>> GetAllAsync();

    Task<SalesQuotationLineDto?> GetByIdAsync(int id);

    Task<SalesQuotationLineDto> CreateAsync(SalesQuotationLineDto dto);

    Task<bool> UpdateAsync(int id, SalesQuotationLineDto dto);

    Task<bool> DeleteAsync(int id);
}
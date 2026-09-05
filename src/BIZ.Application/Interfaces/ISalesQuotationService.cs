using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesQuotationService
{
    Task<IEnumerable<SalesQuotationDto>> GetAllAsync();

    Task<SalesQuotationDto?> GetByIdAsync(int id);

    Task<SalesQuotationDto> CreateAsync(SalesQuotationDto dto);

    Task<bool> UpdateAsync(int id, SalesQuotationDto dto);

    Task<bool> DeleteAsync(int id);
}
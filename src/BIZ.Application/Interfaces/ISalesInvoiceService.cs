using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesInvoiceService
{
    Task<IEnumerable<SalesInvoiceDto>> GetAllAsync();

    Task<SalesInvoiceDto?> GetByIdAsync(int id);

    Task<SalesInvoiceDto> CreateAsync(SalesInvoiceDto dto);

    Task<bool> UpdateAsync(int id, SalesInvoiceDto dto);

    Task<bool> DeleteAsync(int id);
}
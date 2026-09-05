using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISalesInvoiceLineService
{
    Task<IEnumerable<SalesInvoiceLineDto>> GetAllAsync();

    Task<SalesInvoiceLineDto?> GetByIdAsync(int id);

    Task<SalesInvoiceLineDto> CreateAsync(
        SalesInvoiceLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        SalesInvoiceLineDto dto);

    Task<bool> DeleteAsync(int id);
}
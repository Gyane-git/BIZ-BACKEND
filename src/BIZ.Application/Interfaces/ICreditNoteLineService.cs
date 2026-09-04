using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ICreditNoteLineService
{
    Task<List<CreditNoteLineDto>> GetAllAsync();

    Task<List<CreditNoteLineDto>> GetByCreditNoteAsync(int creditNoteId);

    Task<CreditNoteLineDto?> GetByIdAsync(int id);

    Task<CreditNoteLineDto> CreateAsync(CreditNoteLineDto dto);

    Task<bool> UpdateAsync(int id, CreditNoteLineDto dto);

    Task<bool> DeleteAsync(int id);
}
using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IDebitNoteLineService
{
    Task<List<DebitNoteLineDto>> GetAllAsync();

    Task<List<DebitNoteLineDto>> GetByDebitNoteAsync(int debitNoteId);

    Task<DebitNoteLineDto?> GetByIdAsync(int id);

    Task<DebitNoteLineDto> CreateAsync(DebitNoteLineDto dto);

    Task<bool> UpdateAsync(int id, DebitNoteLineDto dto);

    Task<bool> DeleteAsync(int id);
}
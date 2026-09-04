using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IDebitNoteService
{
    Task<List<DebitNoteDto>> GetAllAsync();

    Task<DebitNoteDto?> GetByIdAsync(int id);

    Task<DebitNoteDto?> GetByNumberAsync(string debitNoteNumber);

    Task<List<DebitNoteDto>> GetByFiscalYearAsync(int fiscalYearId);

    Task<List<DebitNoteDto>> GetByPeriodAsync(int fiscalYearPeriodId);

    Task<DebitNoteDto> CreateAsync(DebitNoteDto dto);

    Task<bool> UpdateAsync(int id, DebitNoteDto dto);

    Task<bool> PostAsync(int id);

    Task<bool> DeleteAsync(int id);
}
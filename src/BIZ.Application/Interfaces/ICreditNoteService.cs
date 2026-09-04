using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ICreditNoteService
{
    Task<List<CreditNoteDto>> GetAllAsync();

    Task<CreditNoteDto?> GetByIdAsync(int id);

    Task<CreditNoteDto?> GetByNumberAsync(string creditNoteNumber);

    Task<List<CreditNoteDto>> GetByFiscalYearAsync(int fiscalYearId);

    Task<List<CreditNoteDto>> GetByPeriodAsync(int fiscalYearPeriodId);

    Task<CreditNoteDto> CreateAsync(CreditNoteDto dto);

    Task<bool> UpdateAsync(int id, CreditNoteDto dto);

    Task<bool> PostAsync(int id);

    Task<bool> DeleteAsync(int id);
}
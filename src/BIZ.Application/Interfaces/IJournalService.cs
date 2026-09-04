using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IJournalService
{
    Task<List<JournalDto>> GetAllAsync();

    Task<JournalDto?> GetByIdAsync(int id);

    Task<JournalDto?> GetByNumberAsync(
        string journalNumber);

    Task<List<JournalDto>> GetByFiscalYearAsync(
        int fiscalYearId);

    Task<List<JournalDto>> GetByPeriodAsync(
        int fiscalYearPeriodId);

    Task<JournalDto> CreateAsync(
        JournalDto dto);

    Task<bool> UpdateAsync(
        int id,
        JournalDto dto);

    Task<bool> PostAsync(int id);

    Task<bool> DeleteAsync(int id);
}
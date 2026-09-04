using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IJournalLineService
{
    Task<List<JournalLineDto>> GetAllAsync();

    Task<List<JournalLineDto>> GetByJournalAsync(
        int journalId);

    Task<JournalLineDto?> GetByIdAsync(
        int id);

    Task<JournalLineDto> CreateAsync(
        JournalLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        JournalLineDto dto);

    Task<bool> DeleteAsync(
        int id);
}
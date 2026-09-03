using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISubLedgerService
{
    Task<List<SubLedgerDto>> GetAllAsync();

    Task<List<SubLedgerDto>> GetByLedgerAccountAsync(
        int ledgerAccountId);

    Task<SubLedgerDto?> GetByIdAsync(int id);

    Task<SubLedgerDto?> GetByCodeAsync(string code);

    Task<SubLedgerDto> CreateAsync(SubLedgerDto dto);

    Task<bool> UpdateAsync(
        int id,
        SubLedgerDto dto);

    Task<bool> DeleteAsync(int id);
}
using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ILedgerAccountService
{
    Task<List<LedgerAccountDto>> GetAllAsync();

    Task<List<LedgerAccountDto>> GetByAccountSubGroupAsync(
        int accountSubGroupId);

    Task<LedgerAccountDto?> GetByIdAsync(int id);

    Task<LedgerAccountDto?> GetByCodeAsync(string code);

    Task<LedgerAccountDto> CreateAsync(
        LedgerAccountDto dto);

    Task<bool> UpdateAsync(
        int id,
        LedgerAccountDto dto);

    Task<bool> DeleteAsync(int id);
}
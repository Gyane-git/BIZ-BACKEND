using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ICashAccountService
{
    Task<List<CashAccountDto>> GetAllAsync();

    Task<CashAccountDto?> GetByIdAsync(
        int id);

    Task<CashAccountDto?> GetByCodeAsync(
        string code);

    Task<CashAccountDto> CreateAsync(
        CashAccountDto dto);

    Task<bool> UpdateAsync(
        int id,
        CashAccountDto dto);

    Task<bool> DeleteAsync(
        int id);
}
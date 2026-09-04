using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IBankAccountService
{
    Task<List<BankAccountDto>> GetAllAsync();

    Task<BankAccountDto?> GetByIdAsync(
        int id);

    Task<BankAccountDto?> GetByAccountNumberAsync(
        string accountNumber);

    Task<BankAccountDto> CreateAsync(
        BankAccountDto dto);

    Task<bool> UpdateAsync(
        int id,
        BankAccountDto dto);

    Task<bool> DeleteAsync(
        int id);
}
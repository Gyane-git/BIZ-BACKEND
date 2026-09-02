using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IAccountGroupService
{
    Task<List<AccountGroupDto>> GetAllAsync();

    Task<AccountGroupDto?> GetByIdAsync(int id);

    Task<AccountGroupDto?> GetByCodeAsync(string code);

    Task<AccountGroupDto> CreateAsync(AccountGroupDto dto);

    Task<bool> UpdateAsync(int id, AccountGroupDto dto);

    Task<bool> DeleteAsync(int id);
}
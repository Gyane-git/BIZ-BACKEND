using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IAccountSubGroupService
{
    Task<List<AccountSubGroupDto>> GetAllAsync();

    Task<List<AccountSubGroupDto>> GetByAccountGroupAsync(
        int accountGroupId);

    Task<AccountSubGroupDto?> GetByIdAsync(int id);

    Task<AccountSubGroupDto?> GetByCodeAsync(string code);

    Task<AccountSubGroupDto> CreateAsync(
        AccountSubGroupDto dto);

    Task<bool> UpdateAsync(
        int id,
        AccountSubGroupDto dto);

    Task<bool> DeleteAsync(int id);
}
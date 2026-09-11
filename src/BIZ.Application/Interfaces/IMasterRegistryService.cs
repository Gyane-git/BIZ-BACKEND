using BIZ.Application.DTOs;
using BIZ.Domain.Entities;

namespace BIZ.Application.Interfaces;

public interface IMasterRegistryService
{
    Task<List<Company>> GetCompaniesAsync();
    Task<Company> CreateCompanyAsync(CompanyRequest request);
    Task<bool> UpdateCompanyAsync(int id, CompanyRequest request);
    Task<bool> SetCompanyStatusAsync(int id, bool isActive);
    Task<bool> ProvisionCompanyAsync(int id);
    Task<List<MasterRegistryUserDto>> GetUsersAsync(int? companyId = null);
    Task<MasterRegistryUserDto> CreateUserAsync(UserRequest request);
    Task<bool> UpdateUserAsync(int id, UserUpdateRequest request);
    Task<bool> ResetUserPasswordAsync(int id, UserPasswordResetRequest request);
    Task<bool> SetUserStatusAsync(int id, bool isActive);
    Task<List<Role>> GetRolesAsync();
    Task<Role> CreateRoleAsync(RoleRequest request);
    Task<List<Permission>> GetPermissionsAsync();
    Task<Permission> CreatePermissionAsync(PermissionRequest request);
    Task<List<Permission>> GetRolePermissionsAsync(int roleId);
    Task<List<Permission>> SyncSystemPermissionsAsync();
    Task<bool> AssignRoleAsync(UserRoleRequest request);
    Task<bool> RemoveRoleAsync(UserRoleRequest request);
    Task<bool> AssignPermissionAsync(RolePermissionRequest request);
    Task<bool> RemovePermissionAsync(RolePermissionRequest request);
    Task<List<LoginHistory>> GetLoginHistoryAsync(int take = 100);
    Task<List<AuditLog>> GetAuditLogsAsync(int take = 100);
}

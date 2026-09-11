using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/master-registry")]
[Authorize(Roles = "ADMIN")]
public sealed class MasterRegistryController : ControllerBase
{
    private readonly IMasterRegistryService _service;

    public MasterRegistryController(IMasterRegistryService service) => _service = service;

    [HttpGet("companies")]
    public async Task<IActionResult> Companies() => Ok(await _service.GetCompaniesAsync());

    [HttpPost("companies")]
    public async Task<IActionResult> CreateCompany(CompanyRequest request) => await Execute(() => _service.CreateCompanyAsync(request));

    [HttpPut("companies/{id:int}")]
    public async Task<IActionResult> UpdateCompany(int id, CompanyRequest request) => await Execute(() => _service.UpdateCompanyAsync(id, request));

    [HttpPatch("companies/{id:int}/status")]
    public async Task<IActionResult> CompanyStatus(int id, UserStatusRequest request) => await Execute(() => _service.SetCompanyStatusAsync(id, request.IsActive));

    [HttpPost("companies/{id:int}/provision")]
    public async Task<IActionResult> ProvisionCompany(int id) => await Execute(() => _service.ProvisionCompanyAsync(id));

    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] int? companyId = null) => Ok(await _service.GetUsersAsync(companyId));

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(UserRequest request) => await Execute(() => _service.CreateUserAsync(request));

    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateRequest request) => await Execute(() => _service.UpdateUserAsync(id, request));

    [HttpPatch("users/{id:int}/password")]
    public async Task<IActionResult> ResetUserPassword(int id, UserPasswordResetRequest request) => await Execute(() => _service.ResetUserPasswordAsync(id, request));

    [HttpPatch("users/{id:int}/status")]
    public async Task<IActionResult> UserStatus(int id, UserStatusRequest request) => await Execute(() => _service.SetUserStatusAsync(id, request.IsActive));

    [HttpGet("roles")]
    public async Task<IActionResult> Roles() => Ok(await _service.GetRolesAsync());

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole(RoleRequest request) => await Execute(() => _service.CreateRoleAsync(request));

    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions() => Ok(await _service.GetPermissionsAsync());

    [HttpPost("permissions")]
    public async Task<IActionResult> CreatePermission(PermissionRequest request) => await Execute(() => _service.CreatePermissionAsync(request));

    [HttpGet("roles/{roleId:int}/permissions")]
    public async Task<IActionResult> RolePermissions(int roleId) => Ok(await _service.GetRolePermissionsAsync(roleId));

    [HttpPost("permissions/sync-system")]
    public async Task<IActionResult> SyncSystemPermissions() => Ok(await _service.SyncSystemPermissionsAsync());

    [HttpPost("user-roles")]
    public async Task<IActionResult> AssignRole(UserRoleRequest request) => await Execute(() => _service.AssignRoleAsync(request));

    [HttpDelete("user-roles")]
    public async Task<IActionResult> RemoveRole(UserRoleRequest request) => await Execute(() => _service.RemoveRoleAsync(request));

    [HttpPost("role-permissions")]
    public async Task<IActionResult> AssignPermission(RolePermissionRequest request) => await Execute(() => _service.AssignPermissionAsync(request));

    [HttpDelete("role-permissions")]
    public async Task<IActionResult> RemovePermission(RolePermissionRequest request) => await Execute(() => _service.RemovePermissionAsync(request));

    [HttpGet("login-history")]
    public async Task<IActionResult> LoginHistory([FromQuery] int take = 100) => Ok(await _service.GetLoginHistoryAsync(take));

    [HttpGet("audit-logs")]
    public async Task<IActionResult> AuditLogs([FromQuery] int take = 100) => Ok(await _service.GetAuditLogsAsync(take));

    private static async Task<IActionResult> Execute<T>(Func<Task<T>> action)
    {
        try { return new OkObjectResult(await action()); }
        catch (ArgumentException ex) { return new BadRequestObjectResult(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return new ConflictObjectResult(new { message = ex.Message }); }
    }
}

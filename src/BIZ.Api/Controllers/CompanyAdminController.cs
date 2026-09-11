using BIZ.Application.DTOs;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/company-admin")]
[Authorize]
[CompanyAdminAccess]
public sealed class CompanyAdminController : ControllerBase
{
    private readonly MasterRegistryDbContext _db;

    public CompanyAdminController(MasterRegistryDbContext db) => _db = db;

    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var companyId = CurrentCompanyId();
        return Ok(await _db.Users.AsNoTracking().Where(x => x.CompanyId == companyId).OrderBy(x => x.Username).Select(x => new MasterRegistryUserDto
        {
            Id = x.Id, CompanyId = x.CompanyId, CompanyCode = x.Company.Code, Username = x.Username,
            FullName = x.FullName, IsActive = x.IsActive, CreatedAt = x.CreatedAt,
            Roles = x.UserRoles.Select(r => r.Role.Code).ToList()
        }).ToListAsync());
    }

    [HttpGet("roles")]
    public async Task<IActionResult> Roles()
    {
        var prefix = RolePrefix();
        return Ok(await _db.Roles.AsNoTracking().Where(x => x.Code.StartsWith(prefix) || x.Code == "COMPANY_ADMIN").OrderBy(x => x.Code).ToListAsync());
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions() => Ok(await _db.Permissions.AsNoTracking()
        .Where(x => x.IsActive && x.Code != "PERM_COMPANY_MASTER" && x.Code != "PERM_ROLE_MANAGEMENT" && x.Code != "PERM_PERMISSION_MANAGEMENT")
        .OrderBy(x => x.Code).ToListAsync());

    [HttpGet("roles/{roleId:int}/permissions")]
    public async Task<IActionResult> RolePermissions(int roleId)
    {
        if (!await OwnRole(roleId)) return Forbid();
        return Ok(await _db.RolePermissions.AsNoTracking().Where(x => x.RoleId == roleId && x.Permission.IsActive).Select(x => x.Permission).OrderBy(x => x.Code).ToListAsync());
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole(RoleRequest request)
    {
        var name = Required(request.Name, "Role name");
        var rawCode = Required(request.Code, "Role code").ToUpperInvariant().Replace(" ", "_");
        var code = $"{RolePrefix()}{rawCode}";
        if (await _db.Roles.AnyAsync(x => x.Code == code)) return Conflict(new { message = "Role code already exists in this company." });
        var role = new Role { Code = code, Name = name, Description = request.Description?.Trim(), IsActive = true, CreatedAt = DateTime.UtcNow };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return Ok(role);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(UserRequest request)
    {
        var companyId = CurrentCompanyId();
        var username = Required(request.Username, "Username").ToLowerInvariant();
        if (await _db.Users.AnyAsync(x => x.CompanyId == companyId && x.Username == username)) return Conflict(new { message = "Username already exists in this company." });
        if (request.RoleId.HasValue && !await OwnRole(request.RoleId.Value)) return Forbid();
        var user = new User { CompanyId = companyId, Username = username, FullName = Required(request.FullName, "Full name"), PasswordHash = BCrypt.Net.BCrypt.HashPassword(Required(request.Password, "Password")), IsActive = true, CreatedAt = DateTime.UtcNow };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        if (request.RoleId.HasValue) _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = request.RoleId.Value, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    [HttpPost("role-permissions")]
    public async Task<IActionResult> AssignPermission(RolePermissionRequest request)
    {
        if (!await OwnRole(request.RoleId) || !await _db.Permissions.AnyAsync(x => x.Id == request.PermissionId && x.IsActive && x.Code != "PERM_COMPANY_MASTER" && x.Code != "PERM_ROLE_MANAGEMENT" && x.Code != "PERM_PERMISSION_MANAGEMENT")) return Forbid();
        if (!await _db.RolePermissions.AnyAsync(x => x.RoleId == request.RoleId && x.PermissionId == request.PermissionId))
            _db.RolePermissions.Add(new RolePermission { RoleId = request.RoleId, PermissionId = request.PermissionId, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        return Ok(true);
    }

    [HttpDelete("role-permissions")]
    public async Task<IActionResult> RemovePermission(RolePermissionRequest request)
    {
        if (!await OwnRole(request.RoleId)) return Forbid();
        var link = await _db.RolePermissions.FirstOrDefaultAsync(x => x.RoleId == request.RoleId && x.PermissionId == request.PermissionId);
        if (link != null) { _db.RolePermissions.Remove(link); await _db.SaveChangesAsync(); }
        return Ok(true);
    }

    private int CurrentCompanyId() => int.TryParse(User.FindFirst("companyId")?.Value, out var id) && id > 0 ? id : throw new UnauthorizedAccessException("Company context is missing.");
    private string RolePrefix() => $"C{CurrentCompanyId()}_";
    private Task<bool> OwnRole(int roleId) => _db.Roles.AnyAsync(x => x.Id == roleId && x.IsActive && x.Code.StartsWith(RolePrefix()));
    private static string Required(string? value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} is required.") : value.Trim();
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class CompanyAdminAccessAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        var roles = user.Claims
            .Where(x => x.Type == ClaimTypes.Role || x.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value.Replace(" ", "_").ToUpperInvariant())
            .ToHashSet();
        var permissions = user.Claims
            .Where(x => x.Type.Equals("permission", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allowed = roles.Contains("ADMIN") || roles.Contains("COMPANY_ADMIN") || permissions.Contains("PERM_USER_MANAGEMENT");
        if (!allowed) context.Result = new ForbidResult();
        return Task.CompletedTask;
    }
}

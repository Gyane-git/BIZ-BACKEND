using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using BIZ.Infrastructure.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BIZ.Infrastructure.Services;

public sealed class MasterRegistryService : IMasterRegistryService
{
    private readonly MasterRegistryDbContext _db;
    private readonly TenantDatabaseProvisioner _provisioner;

    public MasterRegistryService(MasterRegistryDbContext db, TenantDatabaseProvisioner provisioner)
    {
        _db = db;
        _provisioner = provisioner;
    }

    public Task<List<Company>> GetCompaniesAsync() =>
        _db.Companies.AsNoTracking().OrderBy(x => x.Code).ToListAsync();

    public async Task<Company> CreateCompanyAsync(CompanyRequest request)
    {
        var code = Required(request.Code, "Company code").ToUpperInvariant();
        if (await _db.Companies.AnyAsync(x => x.Code == code))
            throw new InvalidOperationException("Company code already exists.");
        if (request.SubscriptionEnd.HasValue && request.SubscriptionStart.HasValue &&
            request.SubscriptionEnd < request.SubscriptionStart)
            throw new ArgumentException("Subscription end cannot be earlier than start.");

        var company = new Company
        {
            Code = code,
            Name = Required(request.Name, "Company name"),
            DatabaseServer = Required(request.DatabaseServer, "Database server"),
            DatabaseName = Required(request.DatabaseName, "Database name"),
            SubscriptionPlan = string.IsNullOrWhiteSpace(request.SubscriptionPlan) ? "Basic" : request.SubscriptionPlan.Trim(),
            SubscriptionStart = request.SubscriptionStart,
            SubscriptionEnd = request.SubscriptionEnd,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Companies.Add(company);
        await _db.SaveChangesAsync();
        return company;
    }

    public async Task<bool> UpdateCompanyAsync(int id, CompanyRequest request)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == id);
        if (company == null) return false;
        var code = Required(request.Code, "Company code").ToUpperInvariant();
        if (await _db.Companies.AnyAsync(x => x.Id != id && x.Code == code))
            throw new InvalidOperationException("Company code already exists.");
        company.Code = code;
        company.Name = Required(request.Name, "Company name");
        company.DatabaseServer = Required(request.DatabaseServer, "Database server");
        company.DatabaseName = Required(request.DatabaseName, "Database name");
        company.SubscriptionPlan = string.IsNullOrWhiteSpace(request.SubscriptionPlan) ? "Basic" : request.SubscriptionPlan.Trim();
        company.SubscriptionStart = request.SubscriptionStart;
        company.SubscriptionEnd = request.SubscriptionEnd;
        company.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetCompanyStatusAsync(int id, bool isActive)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == id);
        if (company == null) return false;
        company.IsActive = isActive;
        company.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ProvisionCompanyAsync(int id)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (company == null) return false;
        await _provisioner.ProvisionAsync(company);
        return true;
    }

    public async Task<List<MasterRegistryUserDto>> GetUsersAsync(int? companyId = null) =>
        await _db.Users.AsNoTracking()
            .Where(x => !companyId.HasValue || x.CompanyId == companyId)
            .OrderBy(x => x.Username)
            .Select(x => new MasterRegistryUserDto
            {
                Id = x.Id, CompanyId = x.CompanyId, CompanyCode = x.Company.Code,
                Username = x.Username, FullName = x.FullName, IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                Roles = x.UserRoles.Select(r => r.Role.Code).ToList()
            }).ToListAsync();

    public async Task<MasterRegistryUserDto> CreateUserAsync(UserRequest request)
    {
        var username = Required(request.Username, "Username").ToLowerInvariant();
        if (!await _db.Companies.AnyAsync(x => x.Id == request.CompanyId && x.IsActive))
            throw new ArgumentException("Active company not found.");
        if (request.RoleId.HasValue && !await _db.Roles.AnyAsync(x => x.Id == request.RoleId.Value && x.IsActive))
            throw new ArgumentException("Active role not found.");
        if (await _db.Users.AnyAsync(x => x.CompanyId == request.CompanyId && x.Username == username))
            throw new InvalidOperationException("Username already exists in this company.");

        var user = new User
        {
            CompanyId = request.CompanyId, Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Required(request.Password, "Password")),
            FullName = Required(request.FullName, "Full name"), IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        if (request.RoleId.HasValue)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = request.RoleId.Value, CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();
        }
        return (await GetUsersAsync(request.CompanyId)).Single(x => x.Id == user.Id);
    }

    public async Task<bool> SetUserStatusAsync(int id, bool isActive)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return false;
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(int id, UserUpdateRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return false;
        var username = Required(request.Username, "Username").ToLowerInvariant();
        if (!await _db.Companies.AnyAsync(x => x.Id == request.CompanyId && x.IsActive))
            throw new ArgumentException("Active company not found.");
        if (request.RoleId.HasValue && !await _db.Roles.AnyAsync(x => x.Id == request.RoleId.Value && x.IsActive))
            throw new ArgumentException("Active role not found.");
        if (await _db.Users.AnyAsync(x => x.Id != id && x.CompanyId == request.CompanyId && x.Username == username))
            throw new InvalidOperationException("Username already exists in this company.");

        user.CompanyId = request.CompanyId;
        user.Username = username;
        user.FullName = Required(request.FullName, "Full name");
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        var existingRoles = await _db.UserRoles.Where(x => x.UserId == id).ToListAsync();
        _db.UserRoles.RemoveRange(existingRoles);
        if (request.RoleId.HasValue)
            _db.UserRoles.Add(new UserRole { UserId = id, RoleId = request.RoleId.Value, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetUserPasswordAsync(int id, UserPasswordResetRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Required(request.Password, "Password"));
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<List<Role>> GetRolesAsync() => _db.Roles.AsNoTracking().OrderBy(x => x.Code).ToListAsync();

    public async Task<Role> CreateRoleAsync(RoleRequest request)
    {
        var code = Required(request.Code, "Role code").ToUpperInvariant();
        if (await _db.Roles.AnyAsync(x => x.Code == code)) throw new InvalidOperationException("Role code already exists.");
        var role = new Role { Code = code, Name = Required(request.Name, "Role name"), Description = request.Description?.Trim(), CreatedAt = DateTime.UtcNow };
        _db.Roles.Add(role); await _db.SaveChangesAsync(); return role;
    }

    public Task<List<Permission>> GetPermissionsAsync() => _db.Permissions.AsNoTracking().OrderBy(x => x.Code).ToListAsync();

    public Task<List<Permission>> GetRolePermissionsAsync(int roleId) =>
        _db.RolePermissions.AsNoTracking()
            .Where(x => x.RoleId == roleId && x.Permission.IsActive)
            .Select(x => x.Permission)
            .OrderBy(x => x.Code)
            .ToListAsync();

    public async Task<List<Permission>> SyncSystemPermissionsAsync()
    {
        var existing = await _db.Permissions.ToDictionaryAsync(x => x.Code);
        foreach (var item in SystemPermissionCatalog.All)
        {
            if (existing.ContainsKey(item.Code)) continue;
            var permission = new Permission
            {
                Code = item.Code,
                Name = item.Name,
                Description = item.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.Permissions.Add(permission);
            existing[item.Code] = permission;
        }
        await _db.SaveChangesAsync();
        return existing.Values.OrderBy(x => x.Code).ToList();
    }

    public async Task<Permission> CreatePermissionAsync(PermissionRequest request)
    {
        var code = Required(request.Code, "Permission code").ToUpperInvariant();
        if (await _db.Permissions.AnyAsync(x => x.Code == code)) throw new InvalidOperationException("Permission code already exists.");
        var permission = new Permission { Code = code, Name = Required(request.Name, "Permission name"), Description = request.Description?.Trim(), CreatedAt = DateTime.UtcNow };
        _db.Permissions.Add(permission); await _db.SaveChangesAsync(); return permission;
    }

    public async Task<bool> AssignRoleAsync(UserRoleRequest request)
    {
        if (!await _db.Users.AnyAsync(x => x.Id == request.UserId && x.IsActive) || !await _db.Roles.AnyAsync(x => x.Id == request.RoleId && x.IsActive))
            throw new ArgumentException("Active user and role are required.");
        if (await _db.UserRoles.AnyAsync(x => x.UserId == request.UserId && x.RoleId == request.RoleId)) return false;
        _db.UserRoles.Add(new UserRole { UserId = request.UserId, RoleId = request.RoleId, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(); return true;
    }

    public async Task<bool> RemoveRoleAsync(UserRoleRequest request) => await RemoveAsync(_db.UserRoles, x => x.UserId == request.UserId && x.RoleId == request.RoleId);

    public async Task<bool> AssignPermissionAsync(RolePermissionRequest request)
    {
        if (!await _db.Roles.AnyAsync(x => x.Id == request.RoleId && x.IsActive) || !await _db.Permissions.AnyAsync(x => x.Id == request.PermissionId && x.IsActive))
            throw new ArgumentException("Active role and permission are required.");
        if (await _db.RolePermissions.AnyAsync(x => x.RoleId == request.RoleId && x.PermissionId == request.PermissionId)) return false;
        _db.RolePermissions.Add(new RolePermission { RoleId = request.RoleId, PermissionId = request.PermissionId, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync(); return true;
    }

    public async Task<bool> RemovePermissionAsync(RolePermissionRequest request) => await RemoveAsync(_db.RolePermissions, x => x.RoleId == request.RoleId && x.PermissionId == request.PermissionId);

    public Task<List<LoginHistory>> GetLoginHistoryAsync(int take = 100) => _db.LoginHistory.AsNoTracking().OrderByDescending(x => x.LoginAt).Take(Math.Clamp(take, 1, 500)).ToListAsync();
    public Task<List<AuditLog>> GetAuditLogsAsync(int take = 100) => _db.AuditLogs.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(Math.Clamp(take, 1, 500)).ToListAsync();

    private static string Required(string? value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} is required.") : value.Trim();
    private async Task<bool> RemoveAsync<TEntity>(DbSet<TEntity> set, Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        var entity = await set.FirstOrDefaultAsync(predicate);
        if (entity == null) return false;
        set.Remove(entity); await _db.SaveChangesAsync(); return true;
    }
}

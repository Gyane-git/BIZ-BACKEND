using BIZ.Application.DTOs.Auth;
using BIZ.Application.Interfaces;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BIZ.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly MasterRegistryDbContext _db;
    private readonly IJwtService _jwtService;

    public AuthService(
        MasterRegistryDbContext db,
        IJwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // ============================================================
        // Validate Request
        // ============================================================

        if (string.IsNullOrWhiteSpace(request.CompanyCode))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Company code is required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Username is required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Password is required."
            };
        }

        // ============================================================
        // Find Company
        // ============================================================

        var company = await _db.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Code == request.CompanyCode.Trim() &&
                x.IsActive);

        if (company == null)
        {
            await RecordLoginAsync(null, null, false, "Invalid company code.");
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid company code."
            };
        }

        // ============================================================
        // Find User
        // ============================================================

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CompanyId == company.Id &&
                x.Username == request.Username.Trim() &&
                x.IsActive);

        if (user == null)
        {
            await RecordLoginAsync(null, company.Id, false, "Invalid username or password.");
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }

        // ============================================================
        // Verify Password
        // ============================================================

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            await RecordLoginAsync(user.Id, company.Id, false, "Invalid username or password.");
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }

        // ============================================================
        // Generate JWT
        // ============================================================

        var roles = await _db.UserRoles
            .Where(x => x.UserId == user.Id && x.Role.IsActive)
            .Select(x => x.Role.Code)
            .ToListAsync();

        var permissions = await _db.RolePermissions
            .Where(x => x.Role.IsActive && x.Permission.IsActive &&
                        _db.UserRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == x.RoleId))
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync();

        var token = _jwtService.GenerateToken(
            user.Id,
            user.Username,
            company.Id,
            company.Code,
            company.Name,
            roles,
            permissions);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _db.RefreshTokens.Add(new BIZ.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        });
        _db.LoginHistory.Add(new BIZ.Domain.Entities.LoginHistory
        {
            UserId = user.Id, CompanyId = company.Id, IsSuccess = true, LoginAt = DateTime.UtcNow
        });
        _db.AuditLogs.Add(new BIZ.Domain.Entities.AuditLog
        {
            UserId = user.Id, CompanyId = company.Id, Action = "LOGIN",
            EntityName = "User", EntityId = user.Id.ToString(), CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        // ============================================================
        // Login Success
        // ============================================================

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            ExpiresIn = 60 * 60,
            RefreshToken = refreshToken,
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                CompanyCode = company.Code,
                CompanyName = company.Name
            }
        };
    }

    public async Task<LoginResponse> RefreshAsync(string rawRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            throw new ArgumentException("Refresh token is required.");

        var tokenHash = Hash(rawRefreshToken.Trim());
        var stored = await _db.RefreshTokens
            .Include(x => x.User).ThenInclude(x => x.Company)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
        if (stored == null || stored.RevokedAt.HasValue || stored.ExpiresAt <= DateTime.UtcNow || !stored.User.IsActive || !stored.User.Company.IsActive)
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");

        var roles = await _db.UserRoles.Where(x => x.UserId == stored.UserId && x.Role.IsActive).Select(x => x.Role.Code).ToListAsync();
        var permissions = await _db.RolePermissions
            .Where(x => x.Role.IsActive && x.Permission.IsActive &&
                        _db.UserRoles.Any(ur => ur.UserId == stored.UserId && ur.RoleId == x.RoleId))
            .Select(x => x.Permission.Code).Distinct().ToListAsync();
        var accessToken = _jwtService.GenerateToken(stored.User.Id, stored.User.Username, stored.User.Company.Id, stored.User.Company.Code, stored.User.Company.Name, roles, permissions);
        var replacement = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenHash = Hash(replacement);
        _db.RefreshTokens.Add(new BIZ.Domain.Entities.RefreshToken
        {
            UserId = stored.UserId, TokenHash = Hash(replacement), ExpiresAt = DateTime.UtcNow.AddDays(30), CreatedAt = DateTime.UtcNow
        });
        _db.AuditLogs.Add(new BIZ.Domain.Entities.AuditLog
        {
            UserId = stored.UserId, CompanyId = stored.User.CompanyId, Action = "REFRESH_TOKEN_ROTATED",
            EntityName = "RefreshToken", EntityId = stored.Id.ToString(), CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return new LoginResponse { Success = true, Message = "Token refreshed successfully.", Token = accessToken, RefreshToken = replacement, ExpiresIn = 60 * 60 };
    }

    public async Task<bool> RevokeRefreshTokenAsync(string rawRefreshToken)
    {
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == Hash(rawRefreshToken.Trim()) && !x.RevokedAt.HasValue);
        if (stored == null) return false;
        stored.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task RecordLoginAsync(int? userId, int? companyId, bool success, string reason)
    {
        _db.LoginHistory.Add(new BIZ.Domain.Entities.LoginHistory { UserId = userId, CompanyId = companyId, IsSuccess = success, FailureReason = reason, LoginAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

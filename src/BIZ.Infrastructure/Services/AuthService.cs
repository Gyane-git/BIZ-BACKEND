using BIZ.Application.DTOs.Auth;
using BIZ.Application.Interfaces;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using Microsoft.EntityFrameworkCore;

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
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid username or password."
            };
        }

        // ============================================================
        // Generate JWT
        // ============================================================

        var token = _jwtService.GenerateToken(
            user.Id,
            user.Username,
            company.Id,
            company.Code,
            company.Name);

        // ============================================================
        // Login Success
        // ============================================================

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            ExpiresIn = 60 * 60,
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
}
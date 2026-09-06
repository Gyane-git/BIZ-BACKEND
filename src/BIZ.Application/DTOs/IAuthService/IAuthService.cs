using BIZ.Application.DTOs.Auth;

namespace BIZ.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
}

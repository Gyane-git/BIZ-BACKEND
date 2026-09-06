using BIZ.Application.DTOs.Auth;
using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try { return Ok(await _authService.RefreshAsync(request.RefreshToken)); }
        catch (Exception ex) { return Unauthorized(new { message = ex.Message }); }
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
    {
        var revoked = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        return revoked ? Ok(new { message = "Refresh token revoked." }) : NotFound(new { message = "Refresh token not found." });
    }
}

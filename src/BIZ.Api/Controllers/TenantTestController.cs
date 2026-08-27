using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantTestController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public IActionResult Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var companyId = User.FindFirstValue("companyId");
        var companyCode = User.FindFirstValue("companyCode");
        var companyName = User.FindFirstValue("companyName");

        return Ok(new
        {
            success = true,
            message = "JWT authentication successful.",
            user = new
            {
                userId,
                username
            },
            tenant = new
            {
                companyId,
                companyCode,
                companyName
            }
        });
    }
}
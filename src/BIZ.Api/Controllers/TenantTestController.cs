using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/tenant-test")]
public class TenantTestController : ControllerBase
{
    private readonly TenantDbContext _db;

    public TenantTestController(TenantDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var data = await _db.TenantTests
            .AsNoTracking()
            .ToListAsync();

        return Ok(new
        {
            database = _db.Database.GetDbConnection().Database,
            data
        });
    }
}
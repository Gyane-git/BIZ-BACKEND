using BIZ.Application.Interfaces;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        MasterRegistryDbContext masterRegistryDb,
        ITenantContext tenantContext)
    {
        // Allow Swagger without tenant header
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var companyCode = context.Request.Headers["X-Company-Code"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(companyCode))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "X-Company-Code header is required."
            });

            return;
        }

        companyCode = companyCode.Trim();

        var company = await masterRegistryDb.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Code == companyCode &&
                x.IsActive);

        if (company is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"Company '{companyCode}' was not found or is inactive."
            });

            return;
        }

        tenantContext.SetTenant(
            company.Id,
            company.Code,
            company.Name,
            company.DatabaseServer,
            company.DatabaseName
        );

        await _next(context);
    }
}
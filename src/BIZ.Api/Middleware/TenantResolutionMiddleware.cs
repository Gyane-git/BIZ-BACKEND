using BIZ.Application.Interfaces;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        // ============================================================
        // Allow Swagger without tenant resolution
        // ============================================================

        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // ============================================================
        // Login does not require tenant resolution
        // CompanyCode comes from request body
        // ============================================================

        if (context.Request.Path.StartsWithSegments("/api/Auth/login"))
        {
            await _next(context);
            return;
        }

        // ============================================================
        // Try to get Company Code from JWT
        // ============================================================

        var claims = context.User.Claims
    .Select(c => new
    {
        c.Type,
        c.Value
    })
    .ToList();

var companyCodeFromToken =
    context.User.Claims
        .FirstOrDefault(c =>
            c.Type.Equals(
                "companyCode",
                StringComparison.OrdinalIgnoreCase))
        ?.Value;

        // ============================================================
        // Fallback: X-Company-Code Header
        // Useful for development/testing and non-JWT requests
        // ============================================================

        var companyCodeFromHeader =
            context.Request.Headers["X-Company-Code"]
                .FirstOrDefault();

        var companyCode =
            !string.IsNullOrWhiteSpace(companyCodeFromToken)
                ? companyCodeFromToken
                : companyCodeFromHeader;

        // ============================================================
        // No Tenant Found
        // ============================================================

        if (string.IsNullOrWhiteSpace(companyCode))
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Tenant could not be resolved. Login or provide X-Company-Code."
            });

            return;
        }

        companyCode = companyCode.Trim();

        // ============================================================
        // Find Company in Master Registry
        // ============================================================

        var company = await masterRegistryDb.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Code == companyCode &&
                x.IsActive);

        if (company is null)
        {
            context.Response.StatusCode =
                StatusCodes.Status404NotFound;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message =
                    $"Company '{companyCode}' was not found or is inactive."
            });

            return;
        }

        // ============================================================
        // Security Check
        // ============================================================
        // If JWT contains companyCode, make sure it matches
        // the resolved company.
        // ============================================================

        if (!string.IsNullOrWhiteSpace(companyCodeFromToken) &&
            !string.Equals(
                companyCodeFromToken,
                company.Code,
                StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Tenant access is not valid."
            });

            return;
        }

        // ============================================================
        // Set Current Tenant
        // ============================================================

        tenantContext.SetTenant(
            company.Id,
            company.Code,
            company.Name,
            company.DatabaseServer,
            company.DatabaseName
        );

        // ============================================================
        // Continue Request Pipeline
        // ============================================================

        await _next(context);
    }
}
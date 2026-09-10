using Microsoft.Data.SqlClient;

namespace BIZ.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (SqlException exception)
        {
            _logger.LogError(exception, "Tenant database connection failed for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            var message = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? $"Tenant database operation failed (SQL {exception.Number}): {exception.Message}"
                : "Tenant database is unavailable. Check the company DatabaseServer, DatabaseName, SQL Server status, and tenant database existence.";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message
            });
        }
    }
}

using BIZ.Api.Middleware;
using BIZ.Application.Interfaces;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using BIZ.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using BIZ.Infrastructure.Persistence.Tenant;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Services
// ============================================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TenantDbContext>();

// ============================================================
// Master Registry Database
// ============================================================

builder.Services.AddDbContext<MasterRegistryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MasterRegistry")
    ));

// ============================================================
// Tenant Context
// ============================================================

builder.Services.AddScoped<ITenantContext, TenantContext>();

// ============================================================
// Application
// ============================================================

var app = builder.Build();

// ============================================================
// HTTP Request Pipeline
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Tenant resolution MUST happen before controllers
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
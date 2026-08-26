using BIZ.Infrastructure.Persistence.MasterRegistry;
using Microsoft.EntityFrameworkCore;
using BIZ.Application.Interfaces;
using BIZ.Infrastructure.Tenant;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Services
// ============================================================
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// Database
// ============================================================

builder.Services.AddDbContext<MasterRegistryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MasterRegistry")
    ));

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

app.UseAuthorization();

app.MapControllers();

app.Run();
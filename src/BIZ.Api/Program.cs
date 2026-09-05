using BIZ.Api.Middleware;
using BIZ.Application.Interfaces;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using BIZ.Infrastructure.Persistence.Tenant;
using BIZ.Infrastructure.Seeding;
using BIZ.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using BIZ.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using BIZ.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Services
// ============================================================

builder.Services.AddControllers();
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token. Example: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<TenantDbContext>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ICompanyUnitService, CompanyUnitService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IUnitConversionService, UnitConversionService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProductGroupService, ProductGroupService>();
builder.Services.AddScoped<IProductSubGroupService, ProductSubGroupService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IWarehouseLocationService,WarehouseLocationService>();
builder.Services.AddScoped<IRackService, RackService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ICurrencyRateService,CurrencyRateService>();
builder.Services.AddScoped<IProductUnitService, ProductUnitService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductBarcodeService,ProductBarcodeService>();
builder.Services.AddScoped<IProductImageService,ProductImageService>();
builder.Services.AddScoped<IProductAttributeService,ProductAttributeService>();
builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
builder.Services.AddScoped<IProductBatchService,ProductBatchService>();
builder.Services.AddScoped<IProductSerialService,ProductSerialService>();
builder.Services.AddScoped<IAccountGroupService, AccountGroupService>();
builder.Services.AddScoped<IAccountSubGroupService, AccountSubGroupService>();
builder.Services.AddScoped<ILedgerAccountService,LedgerAccountService>();
builder.Services.AddScoped<ISubLedgerService,SubLedgerService>();
builder.Services.AddScoped<ICostCenterService, CostCenterService>();
builder.Services.AddScoped<IFiscalYearService,FiscalYearService>();
builder.Services.AddScoped<IFiscalYearPeriodService,FiscalYearPeriodService>();
builder.Services.AddScoped< IJournalService,JournalService>();
builder.Services.AddScoped<IJournalLineService,JournalLineService>();
builder.Services.AddScoped<ICashAccountService, CashAccountService>();
builder.Services.AddScoped<IBankAccountService,BankAccountService>();
builder.Services.AddScoped<IPaymentService,PaymentService>();
builder.Services.AddScoped<IReceiptService,ReceiptService>();
builder.Services.AddScoped<ICreditNoteService, CreditNoteService>();
builder.Services.AddScoped<ICreditNoteLineService, CreditNoteLineService>();
builder.Services.AddScoped<IDebitNoteService, DebitNoteService>();
builder.Services.AddScoped<IDebitNoteLineService,DebitNoteLineService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBudgetLineService, BudgetLineService>();
builder.Services.AddScoped<ISalesQuotationService, SalesQuotationService>();
builder.Services.AddScoped<ISalesQuotationLineService,SalesQuotationLineService>();
builder.Services.AddScoped<ISalesOrderService,SalesOrderService>();
builder.Services.AddScoped<ISalesOrderLineService,SalesOrderLineService>();
builder.Services.AddScoped<IDeliveryChallanService,DeliveryChallanService>();
builder.Services.AddScoped<IDeliveryChallanLineService,DeliveryChallanLineService>();
builder.Services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
builder.Services.AddScoped<ISalesInvoiceLineService, SalesInvoiceLineService>();
builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();
builder.Services.AddScoped<ISalesReturnLineService, SalesReturnLineService>();
builder.Services.AddScoped<ISalesPaymentService, SalesPaymentService>();
builder.Services.AddScoped<ISalesPaymentAllocationService, SalesPaymentAllocationService>();


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
// Build Application
// ============================================================

var app = builder.Build();

// ============================================================
// Seed Development Admin User
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<MasterRegistryDbContext>();

    await UserSeeder.SeedAdminUserAsync(db);
}

// ============================================================
// HTTP Request Pipeline
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseHttpsRedirection();

app.UseAuthentication();

app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
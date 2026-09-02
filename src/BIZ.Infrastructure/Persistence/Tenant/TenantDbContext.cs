using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Persistence.Tenant;

public class TenantDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    // ============================================================
    // Tenant Tables
    // ============================================================

    public DbSet<TenantTest> TenantTests => Set<TenantTest>();

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<CompanyUnit> CompanyUnits => Set<CompanyUnit>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductGroup> ProductGroups => Set<ProductGroup>();
    public DbSet<ProductSubGroup> ProductSubGroups => Set<ProductSubGroup>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Model> Models => Set<Model>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations=> Set<WarehouseLocation>();
    public DbSet<Rack> Racks => Set<Rack>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();
    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductBatch> ProductBatches => Set<ProductBatch>();
    public DbSet<ProductSerial> ProductSerials=> Set<ProductSerial>();

    public DbSet<AccountGroup> AccountGroups => Set<AccountGroup>();
    public DbSet<AccountSubGroup> AccountSubGroups => Set<AccountSubGroup>();



    // ============================================================
    // Database Configuration
    // ============================================================

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        if (!_tenantContext.IsResolved)
        {
            throw new InvalidOperationException(
                "Tenant has not been resolved."
            );
        }

        var connectionString =
            $"Server={_tenantContext.DatabaseServer},1433;" +
            $"Database={_tenantContext.DatabaseName};" +
            $"User Id=sa;" +
            $"Password=BIZSql@2026Strong!;" +
            $"TrustServerCertificate=True;" +
            $"Encrypt=False";

        optionsBuilder.UseSqlServer(connectionString);
    }

    // ============================================================
    // Entity Configurations
    // ============================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================================
        // Branch
        // ========================================================

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.Phone)
                .HasMaxLength(50);

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });


        // ========================================================
        // CompanyUnit
        // ========================================================
        modelBuilder.Entity<CompanyUnit>(entity =>
    {
        entity.ToTable("CompanyUnits");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        entity.HasIndex(x => x.Code)
            .IsUnique();

        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Address)
            .HasMaxLength(500);

        entity.Property(x => x.Phone)
            .HasMaxLength(50);

        entity.Property(x => x.Email)
            .HasMaxLength(200);

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .IsRequired();
    });


        // ========================================================
        // Division
        // ========================================================
         modelBuilder.Entity<Division>(entity =>
    {
        entity.ToTable("Divisions");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        entity.HasIndex(x => x.Code)
            .IsUnique();

        entity.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.Description)
            .HasMaxLength(500);

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .IsRequired();
    });


        // ========================================================
        // Department
        // ========================================================
        modelBuilder.Entity<Department>(entity =>
{
    entity.ToTable("Departments");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});


        // ========================================================
        // Region
        // ========================================================
        modelBuilder.Entity<Region>(entity =>
{
    entity.ToTable("Regions");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});



        // ========================================================
        // Area
        // ========================================================
       modelBuilder.Entity<Area>(entity =>
{
    entity.ToTable("Areas");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});

   
        // ========================================================
        // Unit
        // ========================================================
        modelBuilder.Entity<Unit>(entity =>
{
    entity.ToTable("Units");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(x => x.Symbol)
        .HasMaxLength(20);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});

        // ========================================================
        // UnitConversion
        // ========================================================
        modelBuilder.Entity<UnitConversion>(entity =>
{
    entity.ToTable("UnitConversions");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.ConversionFactor)
        .HasPrecision(18, 6)
        .IsRequired();

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.FromUnit)
        .WithMany()
        .HasForeignKey(x => x.FromUnitId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(x => x.ToUnit)
        .WithMany()
        .HasForeignKey(x => x.ToUnitId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(x => new
    {
        x.FromUnitId,
        x.ToUnitId
    })
    .IsUnique();
});
     

     // ========================================================
     // UnitConversion
     // ========================================================
      modelBuilder.Entity<UnitConversion>(entity =>
    {
        entity.ToTable("UnitConversions");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.ConversionFactor)
            .HasPrecision(18, 6)
            .IsRequired();

        entity.Property(x => x.IsActive)
            .IsRequired();

        entity.Property(x => x.CreatedAt)
            .IsRequired();

        entity.HasOne(x => x.FromUnit)
            .WithMany()
            .HasForeignKey(x => x.FromUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ToUnit)
            .WithMany()
            .HasForeignKey(x => x.ToUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new
        {
            x.FromUnitId,
            x.ToUnitId
        })
        .IsUnique();
    });

     // ========================================================
     // ProductCategory
     // ========================================================
     modelBuilder.Entity<ProductCategory>(entity =>
{
    entity.ToTable("ProductCategories");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});



     // ========================================================
     // ProductGroup
     // ========================================================
     modelBuilder.Entity<ProductGroup>(entity =>
{
    entity.ToTable("ProductGroups");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});

     // ========================================================
     // ProductSubGroup
     // ========================================================
    modelBuilder.Entity<ProductSubGroup>(entity =>
{
    entity.ToTable("ProductSubGroups");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => new
    {
        x.ProductGroupId,
        x.Code
    })
    .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.ProductGroup)
        .WithMany(x => x.ProductSubGroups)
        .HasForeignKey(x => x.ProductGroupId)
        .OnDelete(DeleteBehavior.Restrict);
});

        // ========================================================
        // Brand
        // ========================================================
        modelBuilder.Entity<Brand>(entity =>
{
    entity.ToTable("Brands");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});


        // ========================================================
        // Model
        // ========================================================
        modelBuilder.Entity<Model>(entity =>
{
    entity.ToTable("Models");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => new { x.BrandId, x.Code })
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.Brand)
        .WithMany(x => x.Models)
        .HasForeignKey(x => x.BrandId)
        .OnDelete(DeleteBehavior.Restrict);
});

        // ========================================================
        // Customer
        // ========================================================
        modelBuilder.Entity<Customer>(entity =>
{
    entity.ToTable("Customers");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Address)
        .HasMaxLength(500);

    entity.Property(x => x.Phone)
        .HasMaxLength(50);

    entity.Property(x => x.Email)
        .HasMaxLength(200);

    entity.Property(x => x.PanNumber)
        .HasMaxLength(50);

    entity.Property(x => x.ContactPerson)
        .HasMaxLength(200);

    entity.Property(x => x.CreditLimit)
        .HasPrecision(18, 2);

    entity.Property(x => x.CreditDays)
        .IsRequired();

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});

        // ========================================================
        // Supplier
        // ======================================================== 

modelBuilder.Entity<Supplier>(entity =>
{
    entity.ToTable("Suppliers");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Address)
        .HasMaxLength(500);

    entity.Property(x => x.Phone)
        .HasMaxLength(50);

    entity.Property(x => x.Email)
        .HasMaxLength(200);

    entity.Property(x => x.PanNumber)
        .HasMaxLength(50);

    entity.Property(x => x.ContactPerson)
        .HasMaxLength(200);

    entity.Property(x => x.CreditLimit)
        .HasPrecision(18, 2);

    entity.Property(x => x.CreditDays)
        .IsRequired();

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});


   // ========================================================
   // Agent
   // ========================================================

   modelBuilder.Entity<Agent>(entity =>
{
    entity.ToTable("Agents");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Address)
        .HasMaxLength(500);

    entity.Property(x => x.Phone)
        .HasMaxLength(50);

    entity.Property(x => x.Email)
        .HasMaxLength(200);

    entity.Property(x => x.PanNumber)
        .HasMaxLength(50);

    entity.Property(x => x.ContactPerson)
        .HasMaxLength(200);

    entity.Property(x => x.CommissionRate)
        .HasPrecision(5, 2);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});

    // ========================================================
    // Warehouse
    // ========================================================
    modelBuilder.Entity<Warehouse>(entity =>
{
    entity.ToTable("Warehouses");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .HasMaxLength(250);

    entity.Property(x => x.ShortName)
        .HasMaxLength(50);

    entity.Property(x => x.City)
        .HasMaxLength(500);

    entity.Property(x => x.Address)
        .HasMaxLength(500);

    entity.Property(x => x.TelNo)
        .HasMaxLength(50);

    entity.Property(x => x.MobileNo)
        .HasMaxLength(50);

    entity.Property(x => x.ContactPerson)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});


// ========================================================
// WarehouseLocation
// ========================================================
modelBuilder.Entity<WarehouseLocation>(entity =>
{
    entity.ToTable("WarehouseLocations");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Location)
        .HasMaxLength(50);

    entity.Property(x => x.SubLocation)
        .HasMaxLength(50);

    entity.Property(x => x.Rack)
        .HasMaxLength(50);

    entity.Property(x => x.Col)
        .HasMaxLength(50);

    entity.Property(x => x.ActualLocation)
        .HasMaxLength(500);

    entity.Property(x => x.CreatedBy)
        .HasMaxLength(50);

    entity.Property(x => x.CreatedDate);

    entity.Property(x => x.Memo)
        .HasMaxLength(500);

    entity.Property(x => x.LocCode)
        .HasMaxLength(10);

    entity.Property(x => x.Pcode)
        .HasMaxLength(25);

    entity.HasIndex(x => new
    {
        x.WarehouseId,
        x.LocCode
    });

    entity.HasOne(x => x.Warehouse)
        .WithMany(x => x.Locations)
        .HasForeignKey(x => x.WarehouseId)
        .OnDelete(DeleteBehavior.Restrict);
});

// ========================================================
        // Rack
        // ========================================================
        modelBuilder.Entity<Rack>(entity =>
{
    entity.ToTable("Racks");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.Warehouse)
        .WithMany()
        .HasForeignKey(x => x.WarehouseId)
        .OnDelete(DeleteBehavior.Restrict);
});

        // ========================================================
        // Currency
        // ========================================================
        modelBuilder.Entity<Currency>(entity =>
{
    entity.ToTable("Currencies");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(10);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(x => x.Symbol)
        .HasMaxLength(10);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsBaseCurrency)
        .IsRequired();

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});

        // ========================================================
        // CurrencyRate
        // ========================================================
        modelBuilder.Entity<CurrencyRate>(entity =>
{
    entity.ToTable("CurrencyRates");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.RateDate)
        .IsRequired();

    entity.Property(x => x.BuyingRate)
        .HasPrecision(18, 4)
        .IsRequired();

    entity.Property(x => x.SellingRate)
        .HasPrecision(18, 4)
        .IsRequired();

    entity.Property(x => x.AverageRate)
        .HasPrecision(18, 4);

    entity.Property(x => x.Remarks)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasIndex(x => new
    {
        x.CurrencyId,
        x.RateDate
    })
    .IsUnique();

    entity.HasOne(x => x.Currency)
        .WithMany(x => x.CurrencyRates)
        .HasForeignKey(x => x.CurrencyId)
        .OnDelete(DeleteBehavior.Restrict);
});

        // ========================================================
        // Product
        // ======================================================== 

       modelBuilder.Entity<Product>(entity =>
{
    entity.ToTable("Products");

    entity.HasKey(x => x.Id);

    // Basic
    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(25);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(250);

    entity.HasIndex(x => x.Name)
        .IsUnique();

    entity.Property(x => x.ShortName)
        .IsRequired()
        .HasMaxLength(25);

    // Classification
    entity.Property(x => x.Category)
        .HasMaxLength(1);

    entity.Property(x => x.ValuationMethod)
        .HasMaxLength(1);

    entity.Property(x => x.ProductGroupCode)
        .HasMaxLength(15);

    entity.Property(x => x.ProductSubGroupCode)
        .HasMaxLength(15);

    // Pricing
    entity.Property(x => x.MRP)
        .HasPrecision(18, 8);

    entity.Property(x => x.TradeRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.BuyRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.SalesRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.DealerPrice)
        .HasPrecision(16, 6);

    entity.Property(x => x.DiscountRate)
        .HasPrecision(16, 6);

    entity.Property(x => x.Margin)
        .HasPrecision(16, 6);

    // Tax
    entity.Property(x => x.Vat)
        .HasPrecision(10, 6);

    entity.Property(x => x.ExciseRate)
        .HasPrecision(16, 6);

    entity.Property(x => x.BeforeVat)
        .HasPrecision(16, 6);

    // Inventory
    entity.Property(x => x.MaxStock)
        .HasPrecision(18, 8);

    entity.Property(x => x.ReorderLevel)
        .HasPrecision(18, 8);

    entity.Property(x => x.ReorderQty)
        .HasPrecision(18, 8);

    entity.Property(x => x.CurrencyCode)
        .HasMaxLength(30);

    // Other
    entity.Property(x => x.HSCode)
        .HasMaxLength(50);

    // Accounting
    entity.Property(x => x.PurchaseGLCode)
        .HasMaxLength(25);

    entity.Property(x => x.PurchaseReturnGLCode)
        .HasMaxLength(25);

    entity.Property(x => x.SalesGLCode)
        .HasMaxLength(25);

    entity.Property(x => x.SalesReturnGLCode)
        .HasMaxLength(25);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});

        // ========================================================
        // ProductUnit
        // ========================================================
modelBuilder.Entity<ProductUnit>(entity =>
{
    entity.ToTable("ProductUnits");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.ConversionQuantity)
        .HasPrecision(18, 8)
        .IsRequired();

    entity.Property(x => x.PurchaseRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.SalesRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.MRP)
        .HasPrecision(18, 8);

    entity.Property(x => x.IsActive)
        .IsRequired();

    // Product + Unit must be unique
    entity.HasIndex(x => new
    {
        x.ProductId,
        x.UnitId
    })
    .IsUnique();

    // Product relationship
    entity.HasOne(x => x.Product)
        .WithMany(x => x.ProductUnits)
        .HasForeignKey(x => x.ProductId)
        .OnDelete(DeleteBehavior.Restrict);

    // Unit relationship
    entity.HasOne(x => x.Unit)
        .WithMany()
        .HasForeignKey(x => x.UnitId)
        .OnDelete(DeleteBehavior.Restrict);
});


// ========================================================
        // ProductBarcode
        // ========================================================
        modelBuilder.Entity<ProductBarcode>(entity =>
{
    entity.ToTable("ProductBarcodes");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Barcode)
        .IsRequired()
        .HasMaxLength(100);

    entity.HasIndex(x => x.Barcode)
        .IsUnique();

    entity.Property(x => x.IsPrimary)
        .IsRequired();

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.Product)
        .WithMany(x => x.ProductBarcodes)
        .HasForeignKey(x => x.ProductId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(x => x.ProductUnit)
        .WithMany()
        .HasForeignKey(x => x.ProductUnitId)
        .OnDelete(DeleteBehavior.Restrict);
});


        // ========================================================
        // ProductImage
        // ========================================================
        modelBuilder.Entity<ProductImage>(entity =>
{
    entity.ToTable("ProductImages");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.ImageUrl)
        .IsRequired()
        .HasMaxLength(1000);

    entity.Property(x => x.AltText)
        .HasMaxLength(250);

    entity.Property(x => x.IsPrimary)
        .IsRequired();

    entity.Property(x => x.DisplayOrder)
        .IsRequired();

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasIndex(x => new
    {
        x.ProductId,
        x.DisplayOrder
    });

    entity.HasOne(x => x.Product)
        .WithMany(x => x.ProductImages)
        .HasForeignKey(x => x.ProductId)
        .OnDelete(DeleteBehavior.Restrict);
});

// ========================================================
        // ProductAttribute
        // ========================================================
        modelBuilder.Entity<ProductAttribute>(entity =>
{
    entity.ToTable("ProductAttributes");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.AttributeName)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(x => x.AttributeValue)
        .IsRequired()
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasIndex(x => new
    {
        x.ProductId,
        x.AttributeName
    })
    .IsUnique();

    entity.HasOne(x => x.Product)
        .WithMany(x => x.ProductAttributes)
        .HasForeignKey(x => x.ProductId)
        .OnDelete(DeleteBehavior.Restrict);
});

        // ========================================================
        // ProductVariant
        // ========================================================
        modelBuilder.Entity<ProductVariant>(entity =>
{
    entity.ToTable("ProductVariants");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.VariantCode)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.VariantCode)
        .IsUnique();

    entity.Property(x => x.VariantName)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Color)
        .HasMaxLength(100);

    entity.Property(x => x.Size)
        .HasMaxLength(100);

    entity.Property(x => x.Specification)
        .HasMaxLength(500);

    entity.Property(x => x.PurchaseRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.SalesRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.MRP)
        .HasPrecision(18, 8);

    entity.Property(x => x.DealerPrice)
        .HasPrecision(18, 8);

    entity.Property(x => x.DiscountRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.ReorderLevel)
        .HasPrecision(18, 8);

    entity.Property(x => x.ReorderQty)
        .HasPrecision(18, 8);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.Product)
        .WithMany(x => x.ProductVariants)
        .HasForeignKey(x => x.ProductId)
        .OnDelete(DeleteBehavior.Restrict);
});

   // ========================================================
        // ProductBatch
        // ========================================================
       modelBuilder.Entity<ProductBatch>(entity =>
{
    entity.ToTable("ProductBatches");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.BatchNumber)
        .IsRequired()
        .HasMaxLength(100);

    entity.HasIndex(x => new
    {
        x.ProductId,
        x.BatchNumber
    })
    .IsUnique();

    entity.Property(x => x.OpeningQuantity)
        .HasPrecision(18, 8)
        .IsRequired();

    entity.Property(x => x.CurrentQuantity)
        .HasPrecision(18, 8)
        .IsRequired();

    entity.Property(x => x.PurchaseRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.SalesRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.MRP)
        .HasPrecision(18, 8);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.Product)
        .WithMany(x => x.ProductBatches)
        .HasForeignKey(x => x.ProductId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(x => x.ProductVariant)
        .WithMany()
        .HasForeignKey(x => x.ProductVariantId)
        .OnDelete(DeleteBehavior.Restrict);
});


// ========================================================
        // ProductSerial
        // ========================================================
       modelBuilder.Entity<ProductSerial>(entity =>
{
    entity.ToTable("ProductSerials");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.SerialNumber)
        .IsRequired()
        .HasMaxLength(100);

    entity.HasIndex(x => x.SerialNumber)
        .IsUnique();

    entity.Property(x => x.PurchaseRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.SalesRate)
        .HasPrecision(18, 8);

    entity.Property(x => x.Status)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(x => x.Remarks)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.Product)
        .WithMany(x => x.ProductSerials)
        .HasForeignKey(x => x.ProductId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(x => x.ProductVariant)
        .WithMany()
        .HasForeignKey(x => x.ProductVariantId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(x => x.ProductBatch)
        .WithMany()
        .HasForeignKey(x => x.ProductBatchId)
        .OnDelete(DeleteBehavior.Restrict);
});



// ========================================================
        // AccountGroup
        // ========================================================
        modelBuilder.Entity<AccountGroup>(entity =>
{
    entity.ToTable("AccountGroups");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.HasIndex(x => x.Name)
        .IsUnique();

    entity.Property(x => x.Nature)
        .IsRequired()
        .HasMaxLength(30);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();
});
  // ========================================================
        // AccountSubGroup
        // ========================================================
        modelBuilder.Entity<AccountSubGroup>(entity =>
{
    entity.ToTable("AccountSubGroups");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.Code)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(x => x.Code)
        .IsUnique();

    entity.Property(x => x.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(x => x.Description)
        .HasMaxLength(500);

    entity.Property(x => x.IsActive)
        .IsRequired();

    entity.Property(x => x.CreatedAt)
        .IsRequired();

    entity.HasOne(x => x.AccountGroup)
        .WithMany(x => x.AccountSubGroups)
        .HasForeignKey(x => x.AccountGroupId)
        .OnDelete(DeleteBehavior.Restrict);
});







    }
}
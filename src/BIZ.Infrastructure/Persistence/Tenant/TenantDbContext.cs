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


    }
}
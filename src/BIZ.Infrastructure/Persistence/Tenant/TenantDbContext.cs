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
    }
}
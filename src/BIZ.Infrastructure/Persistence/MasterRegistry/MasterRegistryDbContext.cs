using BIZ.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Persistence.MasterRegistry;

public class MasterRegistryDbContext : DbContext
{
    public MasterRegistryDbContext(
        DbContextOptions<MasterRegistryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.DatabaseServer)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.DatabaseName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.SubscriptionPlan)
                .HasMaxLength(50);

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });
    }
}
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.MasterRegistry;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Seeding;

public static class UserSeeder
{
    public static async Task SeedAdminUserAsync(
        MasterRegistryDbContext db)
    {
        const string companyCode = "ERPDEMO1";
        const string username = "admin";
        const string password = "Admin@2026";

        var company = await db.Companies
            .FirstOrDefaultAsync(x => x.Code == companyCode);

        if (company == null)
        {
            throw new InvalidOperationException(
                $"Company '{companyCode}' was not found."
            );
        }

        var existingUser = await db.Users
            .FirstOrDefaultAsync(x =>
                x.CompanyId == company.Id &&
                x.Username == username);

        if (existingUser != null)
        {
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            CompanyId = company.Id,
            Username = username,
            PasswordHash = passwordHash,
            FullName = "BIZ Administrator",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);

        await db.SaveChangesAsync();
    }
}
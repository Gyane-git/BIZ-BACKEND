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

        var adminRole = await db.Roles.FirstOrDefaultAsync(x => x.Code == "ADMIN");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                Code = "ADMIN",
                Name = "Administrator",
                Description = "Full Master Registry administration access.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync();
        }

        if (existingUser != null)
        {
            var assigned = await db.UserRoles.AnyAsync(x => x.UserId == existingUser.Id && x.RoleId == adminRole.Id);
            if (!assigned)
            {
                db.UserRoles.Add(new UserRole { UserId = existingUser.Id, RoleId = adminRole.Id, CreatedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
            }
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
        db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
}

using ArunayanDairy.API.Models;
using ArunayanDairy.API.Security;

namespace ArunayanDairy.API.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if we already have users
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // Seed admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin User",
            Email = "admin@arunayan.com",
            PasswordHash = PasswordHasher.Hash("admin123"),
            Role = "admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}

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

        // Seed test customer user
        var customerUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "customer@test.com",
            PasswordHash = PasswordHasher.Hash("customer123"),
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(adminUser, customerUser);
        context.SaveChanges();

        // Seed categories
        var milkCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Milk",
            Description = "Fresh dairy milk products",
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var dairyCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Dairy Products",
            Description = "Butter, paneer, curd, and other dairy items",
            DisplayOrder = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var beverageCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Beverages",
            Description = "Buttermilk, lassi, and flavored milk",
            DisplayOrder = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Categories.AddRange(milkCategory, dairyCategory, beverageCategory);
        context.SaveChanges();

        // Seed products
        var products = new List<Product>
        {
            // Milk products
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = milkCategory.Id,
                Name = "Full Cream Milk",
                Description = "Rich and creamy full fat milk",
                SKU = "MILK-FC-001",
                Unit = ProductUnit.Liter,
                BasePrice = 60.00m,
                IsActive = true,
                MinOrderQuantity = 0.5m,
                MaxOrderQuantity = 10m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = milkCategory.Id,
                Name = "Toned Milk",
                Description = "Low fat toned milk",
                SKU = "MILK-TN-001",
                Unit = ProductUnit.Liter,
                BasePrice = 50.00m,
                IsActive = true,
                MinOrderQuantity = 0.5m,
                MaxOrderQuantity = 10m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = milkCategory.Id,
                Name = "Double Toned Milk",
                Description = "Extra low fat milk",
                SKU = "MILK-DT-001",
                Unit = ProductUnit.Liter,
                BasePrice = 45.00m,
                IsActive = true,
                MinOrderQuantity = 0.5m,
                MaxOrderQuantity = 10m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // Dairy products
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = dairyCategory.Id,
                Name = "Fresh Paneer",
                Description = "Homemade fresh cottage cheese",
                SKU = "DAIRY-PNR-001",
                Unit = ProductUnit.Kilogram,
                BasePrice = 350.00m,
                IsActive = true,
                MinOrderQuantity = 0.25m,
                MaxOrderQuantity = 5m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = dairyCategory.Id,
                Name = "Fresh Curd",
                Description = "Thick and creamy yogurt",
                SKU = "DAIRY-CRD-001",
                Unit = ProductUnit.Kilogram,
                BasePrice = 60.00m,
                IsActive = true,
                MinOrderQuantity = 0.5m,
                MaxOrderQuantity = 5m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = dairyCategory.Id,
                Name = "White Butter",
                Description = "Fresh churned white butter",
                SKU = "DAIRY-BTR-001",
                Unit = ProductUnit.Kilogram,
                BasePrice = 400.00m,
                IsActive = true,
                MinOrderQuantity = 0.1m,
                MaxOrderQuantity = 2m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = dairyCategory.Id,
                Name = "Ghee",
                Description = "Pure cow ghee",
                SKU = "DAIRY-GHE-001",
                Unit = ProductUnit.Kilogram,
                BasePrice = 550.00m,
                IsActive = true,
                MinOrderQuantity = 0.5m,
                MaxOrderQuantity = 5m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // Beverages
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = beverageCategory.Id,
                Name = "Buttermilk",
                Description = "Fresh and tangy buttermilk",
                SKU = "BEV-BTM-001",
                Unit = ProductUnit.Liter,
                BasePrice = 30.00m,
                IsActive = true,
                MinOrderQuantity = 0.5m,
                MaxOrderQuantity = 5m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = Guid.NewGuid(),
                CategoryId = beverageCategory.Id,
                Name = "Mango Lassi",
                Description = "Sweet mango flavored lassi",
                SKU = "BEV-LSI-001",
                Unit = ProductUnit.Liter,
                BasePrice = 80.00m,
                IsActive = true,
                MinOrderQuantity = 0.5m,
                MaxOrderQuantity = 3m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();

        // Seed product availability for next 7 days
        var today = DateTime.UtcNow.Date;
        var availabilities = new List<ProductAvailability>();

        foreach (var product in products)
        {
            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                var stockQuantity = product.Unit == ProductUnit.Liter ? 100m : 50m;

                availabilities.Add(new ProductAvailability
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    AvailableDate = date,
                    StockQuantity = stockQuantity,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        context.ProductAvailabilities.AddRange(availabilities);
        context.SaveChanges();
    }
}

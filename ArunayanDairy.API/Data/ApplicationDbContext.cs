using Microsoft.EntityFrameworkCore;
using ArunayanDairy.API.Models;

namespace ArunayanDairy.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductAvailability> ProductAvailabilities { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserEntity(modelBuilder);
        ConfigureCategoryEntity(modelBuilder);
        ConfigureProductEntity(modelBuilder);
        ConfigureProductAvailabilityEntity(modelBuilder);
        ConfigureOrderEntity(modelBuilder);
        ConfigureOrderItemEntity(modelBuilder);
    }

    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.HasIndex(e => e.Email)
                .IsUnique();
            
            entity.Property(e => e.PasswordHash)
                .IsRequired();
            
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("user");
            
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500);
        });
    }

    private void ConfigureCategoryEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasMaxLength(500);
            
            entity.Property(e => e.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);
            
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        });
    }

    private void ConfigureProductEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.SKU)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.HasIndex(e => e.SKU)
                .IsUnique();
            
            entity.Property(e => e.Unit)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(e => e.BasePrice)
                .IsRequired()
                .HasPrecision(18, 2);
            
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);
            
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            entity.Property(e => e.MinOrderQuantity)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(1);
            
            entity.Property(e => e.MaxOrderQuantity)
                .HasPrecision(18, 2);
            
            // Foreign key to Category
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Foreign key to User (Vendor)
            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.VendorId);
        });
    }

    private void ConfigureProductAvailabilityEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AvailableDate)
                .IsRequired();
            
            entity.HasIndex(e => e.AvailableDate);
            
            entity.Property(e => e.StockQuantity)
                .IsRequired()
                .HasPrecision(18, 2);
            
            entity.Property(e => e.PriceOverride)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);
            
            // Unique constraint: One availability record per product per date
            entity.HasIndex(e => new { e.ProductId, e.AvailableDate })
                .IsUnique();
            
            // Foreign key to Product
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Availabilities)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureOrderEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.OrderNumber)
                .IsRequired()
                .HasMaxLength(20);
            
            entity.HasIndex(e => e.OrderNumber)
                .IsUnique();
            
            entity.Property(e => e.OrderDate)
                .IsRequired();
            
            entity.HasIndex(e => e.OrderDate);
            
            entity.Property(e => e.DeliveryDate)
                .IsRequired();
            
            entity.HasIndex(e => e.DeliveryDate);
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();
            
            entity.Property(e => e.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);
            
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);
            
            // Foreign key to User (Customer)
            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.CustomerId);
            
            // Foreign key to User (Vendor)
            entity.HasOne(e => e.Vendor)
                .WithMany()
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.VendorId);
        });
    }

    private void ConfigureOrderItemEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Quantity)
                .IsRequired()
                .HasPrecision(18, 2);
            
            entity.Property(e => e.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);
            
            // Foreign key to Order
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Product
            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

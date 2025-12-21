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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
}

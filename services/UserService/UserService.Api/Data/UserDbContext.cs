using Microsoft.EntityFrameworkCore;
using UserService.Api.Models;

namespace UserService.Api.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}

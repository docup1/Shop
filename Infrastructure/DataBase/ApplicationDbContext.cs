using Infrastructure.DataBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Order> Orders { get; set; }

    public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }
}
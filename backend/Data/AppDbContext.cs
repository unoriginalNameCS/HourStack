using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Add your DbSets here, e.g.:
    // public DbSet<YourEntity> YourEntities { get; set; }
}

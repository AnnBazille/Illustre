using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; }

    public DbSet<Image> Images { get; set; }

    public DbSet<ImageProperty> ImageProperties { get; set; }

    public DbSet<Reaction> Reactions { get; set; }

    public DbSet<Tag> Tags { get; set; }
}

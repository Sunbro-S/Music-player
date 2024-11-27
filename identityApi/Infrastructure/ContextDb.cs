using Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ContextDb : DbContext
{
    public ContextDb(DbContextOptions<ContextDb> options) : base(options)
    {
    }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<ExtendedIdentityUser> ExtendedIdentityUsers { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExtendedIdentityUser>(entity =>
        {
            entity.HasKey(e => e.UserName);
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
using FranchiseApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FranchiseApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Franchise> Franchises => Set<Franchise>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Franchise>(entity =>
        {
            entity.Property(f => f.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(f => f.Name).IsUnique();
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(b => new { b.FranchiseId, b.Name }).IsUnique();

            entity.HasOne(b => b.Franchise)
                  .WithMany(f => f.Branches)
                  .HasForeignKey(b => b.FranchiseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(p => new { p.BranchId, p.Name }).IsUnique();

            entity.HasOne(p => p.Branch)
                  .WithMany(b => b.Products)
                  .HasForeignKey(p => p.BranchId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.ToTable(t => t.HasCheckConstraint("CK_Product_Stock_NonNegative", "[Stock] >= 0"));
        });
    }
}

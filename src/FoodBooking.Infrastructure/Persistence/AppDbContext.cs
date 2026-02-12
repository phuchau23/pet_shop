using Microsoft.EntityFrameworkCore;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Phase A - Core Catalog
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    // Authentication
    public DbSet<OtpVerification> OtpVerifications { get; set; }

    // Phase B: Orders, Addresses, Vouchers, Payments
    // Phase C: Shippers, Shippings, Reviews

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
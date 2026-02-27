using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class ProductSizeConfiguration : IEntityTypeConfiguration<ProductSize>
{
    public void Configure(EntityTypeBuilder<ProductSize> builder)
    {
        builder.ToTable("product_sizes");

        builder.HasKey(ps => ps.ProductSizeId);

        builder.Property(ps => ps.ProductSizeId)
            .HasColumnName("product_size_id")
            .ValueGeneratedOnAdd();

        builder.Property(ps => ps.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(ps => ps.Size)
            .HasColumnName("size")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ps => ps.Price)
            .HasColumnName("price")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ps => ps.StockQuantity)
            .HasColumnName("stock_quantity")
            .IsRequired();

        builder.Property(ps => ps.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        // Foreign Key
        builder.HasOne(ps => ps.Product)
            .WithMany(p => p.ProductSizes)
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ps => ps.ProductId);
        builder.HasIndex(ps => new { ps.ProductId, ps.Size }).IsUnique(); // Mỗi product chỉ có 1 size duy nhất
    }
}

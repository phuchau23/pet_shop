using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(pi => pi.ProductImageId);

        builder.Property(pi => pi.ProductImageId)
            .HasColumnName("product_image_id")
            .ValueGeneratedOnAdd();

        builder.Property(pi => pi.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(pi => pi.ImageUrl)
            .HasColumnName("image_url")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(pi => pi.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();

        builder.Property(pi => pi.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        // Foreign Key
        builder.HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index
        builder.HasIndex(pi => pi.ProductId);
    }
}
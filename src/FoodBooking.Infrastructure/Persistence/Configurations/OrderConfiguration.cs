using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(o => o.CustomerPhone)
            .HasColumnName("customer_phone")
            .HasMaxLength(20)
            .IsRequired();

        // Địa chỉ giao hàng
        builder.Property(o => o.AddressDetail)
            .HasColumnName("address_detail")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.WardCode)
            .HasColumnName("ward_code");

        builder.Property(o => o.DistrictCode)
            .HasColumnName("district_code");

        builder.Property(o => o.ProvinceCode)
            .HasColumnName("province_code");

        builder.Property(o => o.FullAddress)
            .HasColumnName("full_address")
            .HasMaxLength(500);

        builder.Property(o => o.CustomerLat)
            .HasColumnName("customer_lat")
            .HasColumnType("double precision");

        builder.Property(o => o.CustomerLng)
            .HasColumnName("customer_lng")
            .HasColumnType("double precision");

        // Điểm xuất phát (shop)
        builder.Property(o => o.ShopLat)
            .HasColumnName("shop_lat")
            .HasColumnType("double precision")
            .HasDefaultValue(10.8506)
            .IsRequired();

        builder.Property(o => o.ShopLng)
            .HasColumnName("shop_lng")
            .HasColumnType("double precision")
            .HasDefaultValue(106.7749)
            .IsRequired();

        // Shipper
        builder.Property(o => o.ShipperId)
            .HasColumnName("shipper_id");

        // Trạng thái
        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .HasDefaultValue("pending")
            .IsRequired();

        // Giá và ghi chú
        builder.Property(o => o.TotalPrice)
            .HasColumnName("total_price")
            .HasPrecision(12, 2);

        builder.Property(o => o.Note)
            .HasColumnName("note")
            .HasColumnType("text");

        // Timestamps
        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Foreign Keys (optional - có thể null)
        builder.HasOne<Province>()
            .WithMany()
            .HasForeignKey(o => o.ProvinceCode)
            .HasPrincipalKey(p => p.Code)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<District>()
            .WithMany()
            .HasForeignKey(o => o.DistrictCode)
            .HasPrincipalKey(d => d.Code)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Ward>()
            .WithMany()
            .HasForeignKey(o => o.WardCode)
            .HasPrincipalKey(w => w.Code)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(o => o.CustomerPhone);
        builder.HasIndex(o => o.ShipperId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
    }
}

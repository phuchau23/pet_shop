using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("vouchers");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(v => v.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(v => v.DiscountType)
            .HasColumnName("discount_type")
            .HasMaxLength(20)
            .HasDefaultValue("percentage")
            .IsRequired();

        builder.Property(v => v.DiscountValue)
            .HasColumnName("discount_value")
            .HasPrecision(12, 2)
            .IsRequired();

        builder.Property(v => v.MinOrderAmount)
            .HasColumnName("min_order_amount")
            .HasPrecision(12, 2);

        builder.Property(v => v.MaxDiscountAmount)
            .HasColumnName("max_discount_amount")
            .HasPrecision(12, 2);

        builder.Property(v => v.UsageLimit)
            .HasColumnName("usage_limit");

        builder.Property(v => v.UsedCount)
            .HasColumnName("used_count")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(v => v.StartDate)
            .HasColumnName("start_date");

        builder.Property(v => v.EndDate)
            .HasColumnName("end_date");

        builder.Property(v => v.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(v => v.Code)
            .IsUnique();
        builder.HasIndex(v => v.IsActive);
        builder.HasIndex(v => v.StartDate);
        builder.HasIndex(v => v.EndDate);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;
using FoodBooking.Domain.Enums;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(p => p.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .HasDefaultValue(PaymentStatus.Unpaid)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.TransactionRef)
            .HasColumnName("transaction_ref")
            .HasMaxLength(100);

        // Generic field để lưu metadata cho các payment gateway khác (JSON format)
        // Ví dụ: {"qrCodeUrl": "...", "paymentUrl": "...", "gatewayOrderId": "..."}
        builder.Property(p => p.PaymentMetadata)
            .HasColumnName("payment_metadata")
            .HasColumnType("text");

        builder.Property(p => p.PaidAt)
            .HasColumnName("paid_at");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Foreign Keys
        builder.HasOne<Order>()
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.OrderId)
            .IsUnique();
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
    }
}

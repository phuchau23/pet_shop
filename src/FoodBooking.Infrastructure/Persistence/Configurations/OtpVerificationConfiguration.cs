using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("otp_verifications");

        builder.HasKey(o => o.OtpVerificationId);

        builder.Property(o => o.OtpVerificationId)
            .HasColumnName("otp_verification_id")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(o => o.OtpCode)
            .HasColumnName("otp_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(o => o.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(o => o.IsUsed)
            .HasColumnName("is_used")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Index for faster lookup
        builder.HasIndex(o => new { o.Email, o.OtpCode, o.IsUsed });
    }
}
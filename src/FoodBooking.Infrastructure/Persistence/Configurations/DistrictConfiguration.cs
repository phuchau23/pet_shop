using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("districts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(d => d.Code)
            .HasColumnName("code")
            .IsRequired();

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Codename)
            .HasColumnName("codename")
            .HasMaxLength(100);

        builder.Property(d => d.DivisionType)
            .HasColumnName("division_type")
            .HasMaxLength(50);

        builder.Property(d => d.ProvinceCode)
            .HasColumnName("province_code")
            .IsRequired();

        // Foreign key relationship
        builder.HasOne(d => d.Province)
            .WithMany(p => p.Districts)
            .HasForeignKey(d => d.ProvinceCode)
            .HasPrincipalKey(p => p.Code)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on Code
        builder.HasIndex(d => d.Code)
            .IsUnique();
    }
}

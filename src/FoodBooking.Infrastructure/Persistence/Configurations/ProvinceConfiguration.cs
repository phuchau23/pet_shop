using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.ToTable("provinces");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Codename)
            .HasColumnName("codename")
            .HasMaxLength(100);

        builder.Property(p => p.DivisionType)
            .HasColumnName("division_type")
            .HasMaxLength(50);

        builder.Property(p => p.PhoneCode)
            .HasColumnName("phone_code");

        builder.Property(p => p.Latitude)
            .HasColumnName("latitude")
            .HasColumnType("double precision");

        builder.Property(p => p.Longitude)
            .HasColumnName("longitude")
            .HasColumnType("double precision");

        // Unique constraint on Code
        builder.HasIndex(p => p.Code)
            .IsUnique();
    }
}

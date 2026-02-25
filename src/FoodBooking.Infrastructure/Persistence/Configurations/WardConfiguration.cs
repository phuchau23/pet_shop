using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FoodBooking.Domain.Entities;

namespace FoodBooking.Infrastructure.Persistence.Configurations;

public class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> builder)
    {
        builder.ToTable("wards");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(w => w.Code)
            .HasColumnName("code")
            .IsRequired();

        builder.Property(w => w.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.Codename)
            .HasColumnName("codename")
            .HasMaxLength(100);

        builder.Property(w => w.DivisionType)
            .HasColumnName("division_type")
            .HasMaxLength(50);

        builder.Property(w => w.DistrictCode)
            .HasColumnName("district_code")
            .IsRequired();

        // Foreign key relationship
        builder.HasOne(w => w.District)
            .WithMany(d => d.Wards)
            .HasForeignKey(w => w.DistrictCode)
            .HasPrincipalKey(d => d.Code)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on Code
        builder.HasIndex(w => w.Code)
            .IsUnique();
    }
}

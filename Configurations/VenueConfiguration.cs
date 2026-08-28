using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable(
            "Venues",
            table => table.HasCheckConstraint(
                "CK_Venues_TotalCapacity",
                "[TotalCapacity] > 0"));

        builder.HasKey(venue => venue.Id);

        builder.Property(venue => venue.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(venue => venue.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(venue => venue.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(venue => new
        {
            venue.Name,
            venue.Address
        }).IsUnique();
    }
}
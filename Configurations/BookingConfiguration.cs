using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class BookingConfiguration
    : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable(
            "Bookings",
            table => table.HasCheckConstraint(
                "CK_Bookings_TotalAmount",
                "[TotalAmount] >= 0"));

        // Primary key
        builder.HasKey(booking => booking.Id);

        // Human-readable booking number
        builder.Property(booking => booking.BookingNumber)
            .IsRequired()
            .HasMaxLength(30);

        // Every booking number must be unique
        builder.HasIndex(booking => booking.BookingNumber)
            .IsUnique();

        // Save enum value as readable text
        builder.Property(booking => booking.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsConcurrencyToken();

        builder.Property(booking => booking.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(booking => booking.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Customer can have multiple bookings
        builder.HasOne(booking => booking.Customer)
            .WithMany(customer => customer.Bookings)
            .HasForeignKey(booking => booking.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Event can have multiple bookings
        builder.HasOne(booking => booking.Event)
            .WithMany(eventItem => eventItem.Bookings)
            .HasForeignKey(booking => booking.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Helps customer booking-history queries
        builder.HasIndex(booking => new
        {
            booking.CustomerId,
            booking.CreatedAt
        });

        // Helps admin event-booking queries
        builder.HasIndex(booking => new
        {
            booking.EventId,
            booking.Status
        });

        // Helps BookingExpiryBackgroundService find expired holds
        builder.HasIndex(booking => new
        {
            booking.Status,
            booking.HoldExpiresAt
        });
    }
}
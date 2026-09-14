using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class BookingSeatConfiguration
    : IEntityTypeConfiguration<BookingSeat>
{
    public void Configure(
        EntityTypeBuilder<BookingSeat> builder)
    {
        builder.ToTable(
            "BookingSeats",
            table => table.HasCheckConstraint(
                "CK_BookingSeats_PriceAtBooking",
                "[PriceAtBooking] >= 0"));

        // Primary key
        builder.HasKey(bookingSeat => bookingSeat.Id);

        builder.Property(bookingSeat => bookingSeat.PriceAtBooking)
            .HasPrecision(18, 2);

        builder.Property(bookingSeat => bookingSeat.IsActive)
            .HasDefaultValue(true)
            .IsConcurrencyToken();

        builder.Property(bookingSeat => bookingSeat.ReservedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // One booking cannot contain the same seat twice.
        builder.HasIndex(bookingSeat => new
        {
            bookingSeat.BookingId,
            bookingSeat.SeatId
        }).IsUnique();

        /*
         * Only one active BookingSeat record can exist for a seat.
         * Released/cancelled historical records can remain in the table.
         */
        builder.HasIndex(bookingSeat => bookingSeat.SeatId)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        // Booking → Many BookingSeats
        builder.HasOne(bookingSeat => bookingSeat.Booking)
            .WithMany(booking => booking.BookingSeats)
            .HasForeignKey(bookingSeat => bookingSeat.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seat → Many historical BookingSeats
        builder.HasOne(bookingSeat => bookingSeat.Seat)
            .WithMany(seat => seat.BookingSeats)
            .HasForeignKey(bookingSeat => bookingSeat.SeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class ParkingReservationConfiguration
    : IEntityTypeConfiguration<ParkingReservation>
{
    public void Configure(
        EntityTypeBuilder<ParkingReservation> builder)
    {
        builder.ToTable(
            "ParkingReservations",
            table => table.HasCheckConstraint(
                "CK_ParkingReservations_Fee",
                "[FeeAtReservation] >= 0"));

        // Primary key
        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.FeeAtReservation)
            .HasPrecision(18, 2);

        builder.Property(reservation => reservation.IsActive)
            .HasDefaultValue(true)
            .IsConcurrencyToken();

        builder.Property(reservation => reservation.ReservedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // One booking can have maximum one parking reservation.
        builder.HasIndex(reservation => reservation.BookingId)
            .IsUnique();

        /*
         * One parking slot can have only one active reservation.
         * Released historical reservations can remain in the table.
         */
        builder.HasIndex(reservation => reservation.ParkingSlotId)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        // Booking → Zero or One ParkingReservation
        builder.HasOne(reservation => reservation.Booking)
            .WithOne(booking => booking.ParkingReservation)
            .HasForeignKey<ParkingReservation>(
                reservation => reservation.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        // ParkingSlot → Many historical reservations
        builder.HasOne(reservation => reservation.ParkingSlot)
            .WithMany(slot => slot.ParkingReservations)
            .HasForeignKey(reservation => reservation.ParkingSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
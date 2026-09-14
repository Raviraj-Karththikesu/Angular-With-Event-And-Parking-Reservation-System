using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable(
            "Seats",
            table => table.HasCheckConstraint(
                "CK_Seats_Price",
                "[Price] >= 0"));

        builder.HasKey(seat => seat.Id);

        builder.Property(seat => seat.SeatNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(seat => seat.RowLabel)
            .HasMaxLength(10);

        builder.Property(seat => seat.SeatType)
            .HasMaxLength(50);

        builder.Property(seat => seat.Price)
            .HasPrecision(18, 2);

        builder.Property(seat => seat.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsConcurrencyToken();

        builder.HasIndex(seat => new
        {
            seat.EventId,
            seat.SeatNumber
        }).IsUnique();

        builder.HasOne(seat => seat.Event)
            .WithMany(eventItem => eventItem.Seats)
            .HasForeignKey(seat => seat.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
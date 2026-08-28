using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable(
            "Payments",
            table => table.HasCheckConstraint(
                "CK_Payments_Amount",
                "[Amount] >= 0"));

        // Primary key
        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.Amount)
            .HasPrecision(18, 2);

        builder.Property(payment => payment.TransactionReference)
            .HasMaxLength(100);

        builder.Property(payment => payment.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsConcurrencyToken();

        builder.Property(payment => payment.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // A booking can have only one payment.
        builder.HasIndex(payment => payment.BookingId)
            .IsUnique();

        // Transaction reference must be unique when it has a value.
        builder.HasIndex(payment => payment.TransactionReference)
            .IsUnique()
            .HasFilter("[TransactionReference] IS NOT NULL");

        // Booking → Zero or One Payment
        builder.HasOne(payment => payment.Booking)
            .WithOne(booking => booking.Payment)
            .HasForeignKey<Payment>(
                payment => payment.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
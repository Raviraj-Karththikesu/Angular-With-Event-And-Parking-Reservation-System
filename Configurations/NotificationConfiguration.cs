using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(
        EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        // Primary key
        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(notification => notification.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(notification => notification.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(notification => notification.IsRead)
            .HasDefaultValue(false);

        builder.Property(notification => notification.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Customer → Many Notifications
        builder.HasOne(notification => notification.Customer)
            .WithMany(customer => customer.Notifications)
            .HasForeignKey(notification => notification.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Helps load one customer's newest notifications.
        builder.HasIndex(notification => new
        {
            notification.CustomerId,
            notification.CreatedAt
        });

        // Helps load unread notifications.
        builder.HasIndex(notification => new
        {
            notification.CustomerId,
            notification.IsRead
        });
    }
}
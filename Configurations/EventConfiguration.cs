using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable(
            "Events",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Events_Capacity",
                    "[Capacity] > 0");

                table.HasCheckConstraint(
                    "CK_Events_TicketPrice",
                    "[TicketPrice] >= 0");

                table.HasCheckConstraint(
                    "CK_Events_ParkingFee",
                    "[ParkingFee] >= 0");

                table.HasCheckConstraint(
                    "CK_Events_DateTime",
                    "[EndDateTime] > [StartDateTime]");
            });

        builder.HasKey(eventItem => eventItem.Id);

        builder.Property(eventItem => eventItem.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(eventItem => eventItem.Description)
            .HasMaxLength(1000);

        builder.Property(eventItem => eventItem.TicketPrice)
            .HasPrecision(18, 2);

        builder.Property(eventItem => eventItem.ParkingFee)
            .HasPrecision(18, 2);

        builder.Property(eventItem => eventItem.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(eventItem => new
        {
            eventItem.VenueId,
            eventItem.StartDateTime,
            eventItem.EndDateTime
        });

        builder.HasOne(eventItem => eventItem.Venue)
            .WithMany(venue => venue.Events)
            .HasForeignKey(eventItem => eventItem.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(eventItem => eventItem.EventCategory)
            .WithMany(category => category.Events)
            .HasForeignKey(eventItem => eventItem.EventCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
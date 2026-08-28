using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class ParkingSlotConfiguration
    : IEntityTypeConfiguration<ParkingSlot>
{
    public void Configure(EntityTypeBuilder<ParkingSlot> builder)
    {
        builder.ToTable(
            "ParkingSlots",
            table => table.HasCheckConstraint(
                "CK_ParkingSlots_Fee",
                "[Fee] >= 0"));

        builder.HasKey(slot => slot.Id);

        builder.Property(slot => slot.SlotNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(slot => slot.Zone)
            .HasMaxLength(50);

        builder.Property(slot => slot.Fee)
            .HasPrecision(18, 2);

        builder.Property(slot => slot.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsConcurrencyToken();

        builder.HasIndex(slot => new
        {
            slot.EventId,
            slot.SlotNumber
        }).IsUnique();

        builder.HasOne(slot => slot.Event)
            .WithMany(eventItem => eventItem.ParkingSlots)
            .HasForeignKey(slot => slot.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
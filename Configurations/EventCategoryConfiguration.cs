using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class EventCategoryConfiguration
    : IEntityTypeConfiguration<EventCategory>
{
    public void Configure(
        EntityTypeBuilder<EventCategory> builder)
    {
        builder.ToTable("EventCategories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(category => category.Name)
            .IsUnique();

        builder.Property(category => category.Description)
            .HasMaxLength(500);

        builder.Property(category => category.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
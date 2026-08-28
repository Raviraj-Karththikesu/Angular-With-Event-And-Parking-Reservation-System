using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Event_and_parking_reservation_system.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(customer => customer.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(customer => customer.Email)
            .IsUnique();

        builder.Property(customer => customer.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(customer => customer.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(customer => customer.Role)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(customer => customer.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(customer => customer.EmailVerificationTokenHash)
            .HasMaxLength(500);

        builder.Property(customer => customer.PasswordResetTokenHash)
            .HasMaxLength(500);

        builder.Property(customer => customer.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
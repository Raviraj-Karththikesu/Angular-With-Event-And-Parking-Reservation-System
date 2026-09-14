using Event_and_parking_reservation_system.Enums;

namespace Event_and_parking_reservation_system.Models;

public class Customer
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Customer;

    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    public bool EmailVerified { get; set; }

    public string? EmailVerificationTokenHash { get; set; }

    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    public string? PasswordResetTokenHash { get; set; }

    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<Booking> Bookings { get; set; }
    = new List<Booking>();

    public ICollection<Notification> Notifications { get; set; }
        = new List<Notification>();
}
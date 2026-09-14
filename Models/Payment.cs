using Event_and_parking_reservation_system.Enums;

namespace Event_and_parking_reservation_system.Models;

public class Payment
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string? TransactionReference { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = null!;
}
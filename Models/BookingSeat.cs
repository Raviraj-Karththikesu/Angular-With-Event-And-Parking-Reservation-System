namespace Event_and_parking_reservation_system.Models;

public class BookingSeat
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int SeatId { get; set; }

    public decimal PriceAtBooking { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReleasedAt { get; set; }

    public Booking Booking { get; set; } = null!;

    public Seat Seat { get; set; } = null!;
}
namespace Event_and_parking_reservation_system.DTOs.BookingSeats;

public class BookingSeatResponseDto
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int SeatId { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public decimal PriceAtBooking { get; set; }

    public bool IsActive { get; set; }

    public DateTime ReservedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }
}
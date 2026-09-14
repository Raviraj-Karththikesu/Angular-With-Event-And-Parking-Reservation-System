using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Enums;

namespace Event_and_parking_reservation_system.Models;

public class Seat
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public string? RowLabel { get; set; }

    public string? SeatType { get; set; }

    public decimal Price { get; set; }

    public SeatStatus Status { get; set; } = SeatStatus.Available;

    public Event Event { get; set; } = null!;

    public ICollection<BookingSeat> BookingSeats { get; set; }
        = new List<BookingSeat>();
}
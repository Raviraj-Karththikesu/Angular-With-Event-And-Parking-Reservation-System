using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Enums;

namespace Event_and_parking_reservation_system.Models;

public class Booking
{
    public int Id { get; set; }

    public string BookingNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }

    public int EventId { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime? HoldExpiresAt { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Customer Customer { get; set; } = null!;

    public Event Event { get; set; } = null!;

    public ICollection<BookingSeat> BookingSeats { get; set; }
        = new List<BookingSeat>();

    public ParkingReservation? ParkingReservation { get; set; }

    public Payment? Payment { get; set; }
}
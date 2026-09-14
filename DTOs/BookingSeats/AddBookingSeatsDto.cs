using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.BookingSeats;

public class AddBookingSeatsDto
{
    [Required]
    [MinLength(1)]
    public List<int> SeatIds { get; set; } = new();
}
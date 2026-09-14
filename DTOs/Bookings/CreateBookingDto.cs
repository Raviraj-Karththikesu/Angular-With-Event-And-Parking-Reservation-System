using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Bookings
{
    public class CreateBookingDto
    {
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one seat is required.")]
        public List<int> SeatIds { get; set; } = new();

        public int? ParkingSlotId { get; set; }
    }
}

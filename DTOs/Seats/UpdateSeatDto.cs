using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Seats
{
    public class UpdateSeatDto
    {
        [Required]
        public string SeatNumber { get; set; } = string.Empty;

        public string? RowLabel { get; set; }

        public string? SeatType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
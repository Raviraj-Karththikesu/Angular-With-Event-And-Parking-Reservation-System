using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Seats
{
    public class GenerateSeatMapDto
    {
        [Required]
        [Range(1, 100)]
        public int Rows { get; set; }

        [Required]
        [Range(1, 100)]
        public int Columns { get; set; }

        public string? SeatType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
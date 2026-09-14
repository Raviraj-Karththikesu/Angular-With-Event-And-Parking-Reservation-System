using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Parking
{
    public class GenerateParkingLayoutDto
    {
        [Required]
        [Range(1, 500)]
        public int TotalSlots { get; set; }

        public string? Zone { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Fee { get; set; }
    }
}
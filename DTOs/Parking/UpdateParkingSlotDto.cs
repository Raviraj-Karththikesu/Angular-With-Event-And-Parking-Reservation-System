using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Parking
{
    public class UpdateParkingSlotDto
    {
        [Required]
        public string SlotNumber { get; set; } = string.Empty;

        public string? Zone { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Fee { get; set; }
    }
}
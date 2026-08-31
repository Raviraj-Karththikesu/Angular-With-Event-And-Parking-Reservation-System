using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Events
{
    public class UpdateEventDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        public int EventCategoryId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TicketPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ParkingFee { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
    }
}
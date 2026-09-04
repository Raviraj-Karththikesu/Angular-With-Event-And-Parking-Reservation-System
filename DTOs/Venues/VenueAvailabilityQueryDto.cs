using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Venues
{
    public class VenueAvailabilityQueryDto
    {
        public int? VenueId { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }
    }
}
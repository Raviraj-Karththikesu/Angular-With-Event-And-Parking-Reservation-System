using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Venues
{
    public class CreateVenueDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TotalCapacity { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Categories
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
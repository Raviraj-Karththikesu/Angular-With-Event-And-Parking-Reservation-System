using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system
    .DTOs.Customers
{
    public class UpdateCustomerProfileDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage =
                "Full name must contain between 2 and 100 characters."
        )]
        public string FullName { get; set; } =
            string.Empty;

        [Phone(
            ErrorMessage = "Enter a valid phone number."
        )]
        [StringLength(
            20,
            ErrorMessage =
                "Phone number cannot exceed 20 characters."
        )]
        public string? PhoneNumber { get; set; }
    }
}
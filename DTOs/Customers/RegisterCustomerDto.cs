using System.ComponentModel.DataAnnotations;

namespace Event_and_parking_reservation_system.DTOs.Customers
{
    public class RegisterCustomerDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Full name must contain between 2 and 100 characters."
        )]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must contain at least 8 characters."
        )]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare(
            nameof(Password),
            ErrorMessage = "Password and confirm password do not match."
        )]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
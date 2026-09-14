namespace Event_and_parking_reservation_system.DTOs.Customers
{
    public class CustomerResponseDto
    {
        public int CustomerId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public CustomerBookingSummaryDto BookingSummary
        {
            get;
            set;
        } = new CustomerBookingSummaryDto();
    }
}
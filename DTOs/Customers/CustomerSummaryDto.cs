namespace Event_and_parking_reservation_system.DTOs.Customers;

public class CustomerSummaryDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool EmailVerified { get; set; }

    public int TotalBookings { get; set; }

    public int UpcomingBookings { get; set; }
}
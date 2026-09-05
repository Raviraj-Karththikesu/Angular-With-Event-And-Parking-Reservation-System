namespace Event_and_parking_reservation_system
    .DTOs.Customers
{
    public class CustomerBookingSummaryDto
    {
        public int TotalBookings { get; set; }

        public int PendingBookings { get; set; }

        public int ConfirmedBookings { get; set; }

        public int CancelledBookings { get; set; }

        public int ExpiredBookings { get; set; }

        public int ActiveUpcomingBookings { get; set; }
    }
}
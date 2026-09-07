namespace Event_and_parking_reservation_system.DTOs.Dashboards
{
    public class CustomerDashboardDto
    {
        public int UpcomingBookings { get; set; }
        public int ReservedParking { get; set; }
        public int RecentPayments { get; set; }
        public int UnreadNotifications { get; set; }
    }
}

namespace Event_and_parking_reservation_system.DTOs.Dashboards
{
    public class AdminDashboardDto
    {
        public int TotalEvents { get; set; }
        public int TotalBookings { get; set; }
        public int AvailableSeats { get; set; }
        public int OccupiedParkingSlots { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
    }
}

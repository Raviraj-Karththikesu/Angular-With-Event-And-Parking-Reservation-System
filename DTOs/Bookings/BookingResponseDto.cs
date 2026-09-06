namespace Event_and_parking_reservation_system.DTOs.Bookings
{
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int EventId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime? HoldExpiresAt { get; set; }
        public int RemainingHoldSeconds { get; set; }
        public List<int> SeatIds { get; set; } = new();
        public int? ParkingSlotId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

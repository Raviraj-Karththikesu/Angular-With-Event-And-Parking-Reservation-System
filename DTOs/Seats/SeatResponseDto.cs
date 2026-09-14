namespace Event_and_parking_reservation_system.DTOs.Seats
{
    public class SeatResponseDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public string SeatNumber { get; set; } = string.Empty;

        public string? RowLabel { get; set; }

        public string? SeatType { get; set; }

        public decimal Price { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
namespace Event_and_parking_reservation_system.DTOs.Payments
{
    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }
}

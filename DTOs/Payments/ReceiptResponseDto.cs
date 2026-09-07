namespace Event_and_parking_reservation_system.DTOs.Payments
{
    public class ReceiptResponseDto
    {
        public int PaymentId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int EventId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
    }
}

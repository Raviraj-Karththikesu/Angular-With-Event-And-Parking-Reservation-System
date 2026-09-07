namespace Event_and_parking_reservation_system.DTOs.Payments
{
    public class PaymentInfoDto
    {
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public decimal AmountDue { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "NotPaid";
        public DateTime? HoldExpiresAt { get; set; }
    }
}

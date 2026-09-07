using Event_and_parking_reservation_system.DTOs.Payments;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentInfoDto> GetPaymentInfoAsync(int requesterCustomerId, bool isAdmin, int bookingId);
        Task<PaymentResponseDto> PayAsync(int requesterCustomerId, bool isAdmin, int bookingId);
        Task<List<PaymentResponseDto>> GetCustomerPaymentsAsync(int requesterCustomerId, bool isAdmin, int customerId);
        Task<ReceiptResponseDto> GetReceiptAsync(int requesterCustomerId, bool isAdmin, int paymentId);
    }
}

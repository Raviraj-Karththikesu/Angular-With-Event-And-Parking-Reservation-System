using Event_and_parking_reservation_system.DTOs.Payments;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Event_and_parking_reservation_system.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly IBookingService _bookingService;
        private readonly INotificationService _notifications;

        public PaymentService(
            IPaymentRepository repository,
            IBookingService bookingService,
            INotificationService notifications)
        {
            _repository = repository;
            _bookingService = bookingService;
            _notifications = notifications;
        }

        public async Task<PaymentInfoDto> GetPaymentInfoAsync(
            int requesterCustomerId,
            bool isAdmin,
            int bookingId)
        {
            Booking booking = await RequireBookingAsync(bookingId);
            EnsureOwnerOrAdmin(requesterCustomerId, isAdmin, booking);

            Payment? payment = await _repository.GetByBookingAsync(bookingId);

            return new PaymentInfoDto
            {
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                AmountDue = booking.TotalAmount,
                BookingStatus = booking.Status.ToString(),
                PaymentStatus = payment?.Status.ToString() ?? "NotPaid",
                HoldExpiresAt = booking.HoldExpiresAt
            };
        }

        public async Task<PaymentResponseDto> PayAsync(
            int requesterCustomerId,
            bool isAdmin,
            int bookingId)
        {
            Booking booking = await RequireBookingAsync(bookingId);
            EnsureOwnerOrAdmin(requesterCustomerId, isAdmin, booking);

            if (booking.Status.ToString().Equals(
                "Expired",
                StringComparison.OrdinalIgnoreCase))
                throw new ConflictException(
                    "This booking has expired and cannot be paid.");

            if (booking.Status.ToString().Equals(
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
                throw new ConflictException(
                    "A cancelled booking cannot be paid.");

            if (booking.HoldExpiresAt.HasValue &&
                booking.HoldExpiresAt.Value <= DateTime.UtcNow)
            {
                await _bookingService.ExpirePendingBookingsAsync();
                throw new ConflictException(
                    "The booking hold expired. Create a new booking.");
            }

            Payment? existing = await _repository.GetByBookingAsync(bookingId);
            if (existing is not null)
                throw new ConflictException(
                    "Payment has already been recorded for this booking.");

            await using IDbContextTransaction transaction =
                await _repository.BeginTransactionAsync();

            try
            {
                DateTime now = DateTime.UtcNow;

                Payment payment = new()
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalAmount,
                    Status = Enum.Parse<PaymentStatus>("Completed", true),
                    PaidAt = now
                };

                await _repository.AddAsync(payment);

                booking.Status =
                    Enum.Parse<BookingStatus>("Confirmed", true);
                booking.HoldExpiresAt = null;
                booking.UpdatedAt = now;

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();

                await _notifications.CreateInternalAsync(
                    booking.CustomerId,
                    "PaymentCompleted",
                    "Payment completed",
                    $"Payment completed for booking {booking.BookingNumber}.");

                await _notifications.CreateInternalAsync(
                    booking.CustomerId,
                    "BookingConfirmed",
                    "Booking confirmed",
                    $"Booking {booking.BookingNumber} is confirmed.");

                return Map(payment);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<PaymentResponseDto>> GetCustomerPaymentsAsync(
            int requesterCustomerId,
            bool isAdmin,
            int customerId)
        {
            if (!isAdmin && requesterCustomerId != customerId)
                throw new AppException(
                    "You can only view your own payment history.",
                    StatusCodes.Status403Forbidden);

            List<Payment> payments =
                await _repository.GetByCustomerAsync(customerId);

            return payments.Select(Map).ToList();
        }

        public async Task<ReceiptResponseDto> GetReceiptAsync(
            int requesterCustomerId,
            bool isAdmin,
            int paymentId)
        {
            Payment? payment = await _repository.GetByIdAsync(paymentId);

            if (payment is null)
                throw new NotFoundException("Payment was not found.");

            EnsureOwnerOrAdmin(
                requesterCustomerId,
                isAdmin,
                payment.Booking);

            if (!payment.Status.ToString().Equals(
                "Completed",
                StringComparison.OrdinalIgnoreCase))
                throw new ConflictException(
                    "A receipt is only available for a completed payment.");

            return new ReceiptResponseDto
            {
                PaymentId = payment.Id,
                BookingNumber = payment.Booking.BookingNumber,
                CustomerId = payment.Booking.CustomerId,
                EventId = payment.Booking.EventId,
                AmountPaid = payment.Amount,
                PaymentStatus = payment.Status.ToString(),
                PaidAt = payment.PaidAt
            };
        }

        private async Task<Booking> RequireBookingAsync(int bookingId)
        {
            Booking? booking = await _repository.GetBookingAsync(bookingId);
            return booking ?? throw new NotFoundException("Booking was not found.");
        }

        private static void EnsureOwnerOrAdmin(
            int requesterCustomerId,
            bool isAdmin,
            Booking booking)
        {
            if (!isAdmin && booking.CustomerId != requesterCustomerId)
                throw new AppException(
                    "You can only access your own payment.",
                    StatusCodes.Status403Forbidden);
        }

        private static PaymentResponseDto Map(Payment payment) =>
            new()
            {
                PaymentId = payment.Id,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                PaidAt = payment.PaidAt
            };
    }
}

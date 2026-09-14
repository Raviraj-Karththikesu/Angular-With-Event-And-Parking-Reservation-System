using Event_and_parking_reservation_system.DTOs.Bookings;

namespace Event_and_parking_reservation_system.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateAsync(int customerId, CreateBookingDto dto);
        Task<BookingResponseDto> GetByIdAsync(int requesterCustomerId, bool isAdmin, int bookingId);
        Task<List<BookingResponseDto>> GetCustomerBookingsAsync(int requesterCustomerId, bool isAdmin, int customerId);
        Task<List<BookingResponseDto>> GetEventBookingsAsync(int eventId);
        Task<HoldStatusDto> GetHoldStatusAsync(int requesterCustomerId, bool isAdmin, int bookingId);
        Task CancelAsync(int requesterCustomerId, bool isAdmin, int bookingId);
        Task<int> ExpirePendingBookingsAsync();
    }
}

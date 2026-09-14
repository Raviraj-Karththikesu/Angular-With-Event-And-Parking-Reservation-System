using Event_and_parking_reservation_system.DTOs.BookingSeats;

namespace Event_and_parking_reservation_system.Interfaces.Services;

public interface IBookingSeatService
{
    Task<List<BookingSeatResponseDto>> AddSeatsToBookingAsync(
        int bookingId,
        AddBookingSeatsDto dto);

    Task<List<BookingSeatResponseDto>> GetBookingSeatsAsync(
        int bookingId);
}
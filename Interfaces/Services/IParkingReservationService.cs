using Event_and_parking_reservation_system.DTOs.ParkingReservations;

namespace Event_and_parking_reservation_system.Interfaces.Services;

public interface IParkingReservationService
{
    Task<ParkingReservationResponseDto> ReserveParkingAsync(
        CreateParkingReservationDto dto);

    Task<ParkingReservationResponseDto?> GetByBookingIdAsync(
        int bookingId);

    Task ReleaseParkingAsync(
        int bookingId);
}
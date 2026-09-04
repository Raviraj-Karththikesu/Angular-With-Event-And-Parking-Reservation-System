using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Interfaces.Repositories;

public interface IParkingReservationRepository
{
    Task<ParkingReservation?> GetByBookingIdAsync(int bookingId);

    Task<ParkingReservation?> GetByIdAsync(int reservationId);

    Task<bool> HasActiveReservationAsync(int bookingId);

    Task<bool> IsSlotActivelyReservedAsync(int parkingSlotId);

    Task AddAsync(ParkingReservation reservation);

    void Update(ParkingReservation reservation);

    Task SaveChangesAsync();
}
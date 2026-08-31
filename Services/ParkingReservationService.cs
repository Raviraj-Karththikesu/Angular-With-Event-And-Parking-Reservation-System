using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.ParkingReservations;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Services;

public class ParkingReservationService : IParkingReservationService
{
    private readonly IParkingReservationRepository _parkingReservationRepository;
    private readonly AppDbContext _context;

    public ParkingReservationService(
        IParkingReservationRepository parkingReservationRepository,
        AppDbContext context)
    {
        _parkingReservationRepository = parkingReservationRepository;
        _context = context;
    }

    public async Task<ParkingReservationResponseDto> ReserveParkingAsync(
        CreateParkingReservationDto dto)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == dto.BookingId);

        if (booking == null)
            throw new KeyNotFoundException("Booking not found.");

        var parkingSlot = await _context.ParkingSlots
            .FirstOrDefaultAsync(p => p.Id == dto.ParkingSlotId);

        if (parkingSlot == null)
            throw new KeyNotFoundException("Parking slot not found.");

        if (parkingSlot.EventId != booking.EventId)
            throw new InvalidOperationException(
                "Parking slot does not belong to the booked event.");

        if (await _parkingReservationRepository.HasActiveReservationAsync(dto.BookingId))
            throw new InvalidOperationException(
                "This booking already has an active parking reservation.");

        if (parkingSlot.Status != ParkingSlotStatus.Available)
            throw new InvalidOperationException(
                "Parking slot is not available.");

        if (await _parkingReservationRepository.IsSlotActivelyReservedAsync(dto.ParkingSlotId))
            throw new InvalidOperationException(
                "Parking slot is already reserved.");

        var reservation = new ParkingReservation
        {
            BookingId = dto.BookingId,
            ParkingSlotId = dto.ParkingSlotId,
            FeeAtReservation = parkingSlot.Fee,
            IsActive = true,
            ReservedAt = DateTime.UtcNow
        };

        parkingSlot.Status = ParkingSlotStatus.Reserved;

        await _parkingReservationRepository.AddAsync(reservation);
        await _parkingReservationRepository.SaveChangesAsync();

        reservation.ParkingSlot = parkingSlot;

        return MapToResponse(reservation);
    }

    public async Task<ParkingReservationResponseDto?> GetByBookingIdAsync(
        int bookingId)
    {
        var reservation =
            await _parkingReservationRepository.GetByBookingIdAsync(bookingId);

        if (reservation == null)
            return null;

        return MapToResponse(reservation);
    }

    public async Task ReleaseParkingAsync(int bookingId)
    {
        var reservation =
            await _parkingReservationRepository.GetByBookingIdAsync(bookingId);

        if (reservation == null || !reservation.IsActive)
            throw new KeyNotFoundException(
                "Active parking reservation not found.");

        reservation.IsActive = false;
        reservation.ReleasedAt = DateTime.UtcNow;

        reservation.ParkingSlot.Status =
            ParkingSlotStatus.Available;

        _parkingReservationRepository.Update(reservation);

        await _parkingReservationRepository.SaveChangesAsync();
    }

    private static ParkingReservationResponseDto MapToResponse(
        ParkingReservation reservation)
    {
        return new ParkingReservationResponseDto
        {
            Id = reservation.Id,
            BookingId = reservation.BookingId,
            ParkingSlotId = reservation.ParkingSlotId,
            SlotNumber = reservation.ParkingSlot.SlotNumber,
            Zone = reservation.ParkingSlot.Zone,
            FeeAtReservation = reservation.FeeAtReservation,
            IsActive = reservation.IsActive,
            ReservedAt = reservation.ReservedAt,
            ReleasedAt = reservation.ReleasedAt
        };
    }
}
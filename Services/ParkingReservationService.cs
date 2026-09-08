using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs
    .ParkingReservations;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system
    .Interfaces.Repositories;
using Event_and_parking_reservation_system
    .Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Services;

public class ParkingReservationService :
    IParkingReservationService
{
    private readonly IParkingReservationRepository
        _parkingReservationRepository;

    private readonly AppDbContext _context;

    public ParkingReservationService(
        IParkingReservationRepository
            parkingReservationRepository,
        AppDbContext context)
    {
        _parkingReservationRepository =
            parkingReservationRepository;

        _context = context;
    }

    public async Task<ParkingReservationResponseDto>
        ReserveParkingAsync(
            CreateParkingReservationDto dto)
    {
        Booking? booking =
            await _context.Bookings
                .FirstOrDefaultAsync(item =>
                    item.Id == dto.BookingId
                );

        if (booking is null)
        {
            throw new NotFoundException(
                "Booking not found."
            );
        }

        ParkingSlot? parkingSlot =
            await _context.ParkingSlots
                .FirstOrDefaultAsync(item =>
                    item.Id == dto.ParkingSlotId
                );

        if (parkingSlot is null)
        {
            throw new NotFoundException(
                "Parking slot not found."
            );
        }

        if (parkingSlot.EventId != booking.EventId)
        {
            throw new ConflictException(
                "Parking slot does not belong to the booked event."
            );
        }

        bool bookingAlreadyHasParking =
            await _parkingReservationRepository
                .HasActiveReservationAsync(
                    dto.BookingId
                );

        if (bookingAlreadyHasParking)
        {
            throw new ConflictException(
                "This booking already has an active parking reservation."
            );
        }

        if (parkingSlot.Status !=
            ParkingSlotStatus.Available)
        {
            throw new ConflictException(
                "Parking slot is not available."
            );
        }

        bool slotAlreadyReserved =
            await _parkingReservationRepository
                .IsSlotActivelyReservedAsync(
                    dto.ParkingSlotId
                );

        if (slotAlreadyReserved)
        {
            throw new ConflictException(
                "Parking slot is already reserved."
            );
        }

        ParkingReservation reservation = new()
        {
            BookingId = dto.BookingId,
            ParkingSlotId = dto.ParkingSlotId,
            FeeAtReservation = parkingSlot.Fee,
            IsActive = true,
            ReservedAt = DateTime.UtcNow
        };

        parkingSlot.Status =
            ParkingSlotStatus.Reserved;

        try
        {
            await _parkingReservationRepository
                .AddAsync(reservation);

            await _parkingReservationRepository
                .SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Parking reservation changed by another request. " +
                "Please refresh and try again."
            );
        }
        catch (DbUpdateException)
        {
            throw new ConflictException(
                "Parking slot is no longer available. " +
                "Please refresh the parking layout."
            );
        }

        reservation.ParkingSlot = parkingSlot;

        return MapToResponse(reservation);
    }

    public async Task<
        ParkingReservationResponseDto?>
        GetByBookingIdAsync(int bookingId)
    {
        ParkingReservation? reservation =
            await _parkingReservationRepository
                .GetByBookingIdAsync(bookingId);

        if (reservation is null)
        {
            return null;
        }

        return MapToResponse(reservation);
    }

    public async Task ReleaseParkingAsync(
        int bookingId)
    {
        ParkingReservation? reservation =
            await _parkingReservationRepository
                .GetByBookingIdAsync(bookingId);

        if (reservation is null ||
            !reservation.IsActive)
        {
            throw new NotFoundException(
                "Active parking reservation not found."
            );
        }

        reservation.IsActive = false;
        reservation.ReleasedAt = DateTime.UtcNow;

        reservation.ParkingSlot.Status =
            ParkingSlotStatus.Available;

        _parkingReservationRepository.Update(
            reservation
        );

        try
        {
            await _parkingReservationRepository
                .SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Parking reservation changed by another request. " +
                "Please refresh and try again."
            );
        }
    }

    private static ParkingReservationResponseDto
        MapToResponse(
            ParkingReservation reservation)
    {
        return new ParkingReservationResponseDto
        {
            Id = reservation.Id,
            BookingId = reservation.BookingId,
            ParkingSlotId =
                reservation.ParkingSlotId,
            SlotNumber =
                reservation.ParkingSlot.SlotNumber,
            Zone = reservation.ParkingSlot.Zone,
            FeeAtReservation =
                reservation.FeeAtReservation,
            IsActive = reservation.IsActive,
            ReservedAt = reservation.ReservedAt,
            ReleasedAt = reservation.ReleasedAt
        };
    }
}
using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.BookingSeats;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Services;

public class BookingSeatService : IBookingSeatService
{
    private readonly IBookingSeatRepository _bookingSeatRepository;
    private readonly AppDbContext _context;

    public BookingSeatService(
        IBookingSeatRepository bookingSeatRepository,
        AppDbContext context)
    {
        _bookingSeatRepository = bookingSeatRepository;
        _context = context;
    }

    public async Task<List<BookingSeatResponseDto>> AddSeatsToBookingAsync(
        int bookingId,
        AddBookingSeatsDto dto)
    {
        if (dto.SeatIds.Count != dto.SeatIds.Distinct().Count())
        {
            throw new ConflictException(
                "The same seat cannot be selected more than once.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                throw new NotFoundException("Booking not found.");
            }

            var seats = await _context.Seats
                .Where(s => dto.SeatIds.Contains(s.Id))
                .ToListAsync();

            if (seats.Count != dto.SeatIds.Count)
            {
                throw new NotFoundException(
                    "One or more selected seats were not found.");
            }

            if (seats.Any(s => s.EventId != booking.EventId))
            {
                throw new ConflictException(
                    "One or more seats do not belong to the booking event.");
            }

            var unavailableSeatIds = new List<int>();

            foreach (var seat in seats)
            {
                bool alreadyReserved =
                    await _bookingSeatRepository
                        .HasActiveSeatAsync(seat.Id);

                if (seat.Status != SeatStatus.Available ||
                    alreadyReserved)
                {
                    unavailableSeatIds.Add(seat.Id);
                }
            }

            if (unavailableSeatIds.Count > 0)
            {
                throw new ConflictException(
                    $"One or more seats are already booked: {string.Join(", ", unavailableSeatIds)}");
            }

            var bookingSeats = seats
                .Select(seat => new BookingSeat
                {
                    BookingId = bookingId,
                    SeatId = seat.Id,
                    PriceAtBooking = seat.Price,
                    IsActive = true,
                    ReservedAt = DateTime.UtcNow,
                    Seat = seat
                })
                .ToList();

            foreach (var seat in seats)
            {
                seat.Status = SeatStatus.Booked;
            }

            await _bookingSeatRepository
                .AddRangeAsync(bookingSeats);

            await _bookingSeatRepository
                .SaveChangesAsync();

            await transaction.CommitAsync();

            return bookingSeats
                .Select(MapToResponse)
                .ToList();
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            throw new ConflictException(
                "One or more selected seats were booked by another customer. Please refresh the seat map.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            throw new ConflictException(
                "One or more selected seats are no longer available.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<BookingSeatResponseDto>> GetBookingSeatsAsync(
        int bookingId)
    {
        bool bookingExists =
            await _context.Bookings.AnyAsync(b => b.Id == bookingId);

        if (!bookingExists)
        {
            throw new NotFoundException("Booking not found.");
        }

        var bookingSeats =
            await _bookingSeatRepository.GetByBookingIdAsync(bookingId);

        return bookingSeats
            .Select(MapToResponse)
            .ToList();
    }

    private static BookingSeatResponseDto MapToResponse(
        BookingSeat bookingSeat)
    {
        return new BookingSeatResponseDto
        {
            Id = bookingSeat.Id,
            BookingId = bookingSeat.BookingId,
            SeatId = bookingSeat.SeatId,
            SeatNumber = bookingSeat.Seat.SeatNumber,
            PriceAtBooking = bookingSeat.PriceAtBooking,
            IsActive = bookingSeat.IsActive,
            ReservedAt = bookingSeat.ReservedAt,
            ReleasedAt = bookingSeat.ReleasedAt
        };
    }
}
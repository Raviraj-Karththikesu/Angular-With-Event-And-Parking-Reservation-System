using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.Seats;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Services
{
    public class SeatService : ISeatService
    {
        private readonly ISeatRepository _seatRepository;
        private readonly AppDbContext _context;

        public SeatService(
            ISeatRepository seatRepository,
            AppDbContext context)
        {
            _seatRepository = seatRepository;
            _context = context;
        }

        public async Task<List<SeatResponseDto>> GetSeatMapAsync(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.Id == eventId);

            if (!eventExists)
                throw new KeyNotFoundException("Event not found.");

            var seats = await _seatRepository.GetByEventIdAsync(eventId);

            return seats.Select(MapToResponse).ToList();
        }

        public async Task<List<SeatResponseDto>> GenerateSeatMapAsync(
            int eventId,
            GenerateSeatMapDto dto)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found.");

            var seatCount = dto.Rows * dto.Columns;

            if (seatCount != eventEntity.Capacity)
                throw new InvalidOperationException(
                    $"Seat count must exactly match event capacity of {eventEntity.Capacity}.");

            if (await _seatRepository.SeatMapExistsAsync(eventId))
                throw new InvalidOperationException(
                    "Seat map already exists for this event.");

            var seats = new List<Seat>();

            for (int row = 0; row < dto.Rows; row++)
            {
                string rowLabel = ((char)('A' + row)).ToString();

                for (int column = 1; column <= dto.Columns; column++)
                {
                    seats.Add(new Seat
                    {
                        EventId = eventId,
                        SeatNumber = $"{rowLabel}{column}",
                        RowLabel = rowLabel,
                        SeatType = dto.SeatType,
                        Price = dto.Price > 0
                            ? dto.Price
                            : eventEntity.TicketPrice,
                        Status = SeatStatus.Available
                    });
                }
            }

            await _seatRepository.AddRangeAsync(seats);
            await _seatRepository.SaveChangesAsync();

            return seats.Select(MapToResponse).ToList();
        }

        public async Task<SeatResponseDto> UpdateSeatAsync(
            int eventId,
            int seatId,
            UpdateSeatDto dto)
        {
            var seat = await _seatRepository.GetByIdAsync(seatId);

            if (seat == null || seat.EventId != eventId)
                throw new KeyNotFoundException("Seat not found.");

            if (seat.BookingSeats.Any())
                throw new InvalidOperationException(
                    "A booked seat cannot be edited.");

            seat.SeatNumber = dto.SeatNumber;
            seat.RowLabel = dto.RowLabel;
            seat.SeatType = dto.SeatType;
            seat.Price = dto.Price;

            _seatRepository.Update(seat);
            await _seatRepository.SaveChangesAsync();

            return MapToResponse(seat);
        }

        public async Task DeleteSeatAsync(
            int eventId,
            int seatId)
        {
            var seat = await _seatRepository.GetByIdAsync(seatId);

            if (seat == null || seat.EventId != eventId)
                throw new KeyNotFoundException("Seat not found.");

            if (seat.BookingSeats.Any())
                throw new InvalidOperationException(
                    "A booked seat cannot be deleted.");

            _seatRepository.Remove(seat);
            await _seatRepository.SaveChangesAsync();
        }

        private static SeatResponseDto MapToResponse(Seat seat)
        {
            return new SeatResponseDto
            {
                Id = seat.Id,
                EventId = seat.EventId,
                SeatNumber = seat.SeatNumber,
                RowLabel = seat.RowLabel,
                SeatType = seat.SeatType,
                Price = seat.Price,
                Status = seat.Status.ToString()
            };
        }
    }
}
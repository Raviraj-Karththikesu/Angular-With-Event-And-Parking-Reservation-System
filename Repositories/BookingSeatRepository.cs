using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories;

public class BookingSeatRepository : IBookingSeatRepository
{
    private readonly AppDbContext _context;

    public BookingSeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookingSeat>> GetByBookingIdAsync(
        int bookingId)
    {
        return await _context.BookingSeats
            .Include(bs => bs.Seat)
            .Where(bs =>
                bs.BookingId == bookingId &&
                bs.IsActive)
            .ToListAsync();
    }

    public async Task<bool> HasActiveSeatAsync(int seatId)
    {
        return await _context.BookingSeats
            .AnyAsync(bs =>
                bs.SeatId == seatId &&
                bs.IsActive);
    }

    public async Task AddRangeAsync(
        IEnumerable<BookingSeat> bookingSeats)
    {
        await _context.BookingSeats
            .AddRangeAsync(bookingSeats);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
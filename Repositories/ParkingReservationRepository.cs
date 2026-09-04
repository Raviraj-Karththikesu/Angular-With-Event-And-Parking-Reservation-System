using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories;

public class ParkingReservationRepository : IParkingReservationRepository
{
    private readonly AppDbContext _context;

    public ParkingReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ParkingReservation?> GetByBookingIdAsync(int bookingId)
    {
        return await _context.ParkingReservations
            .Include(r => r.ParkingSlot)
            .FirstOrDefaultAsync(r => r.BookingId == bookingId);
    }

    public async Task<ParkingReservation?> GetByIdAsync(int reservationId)
    {
        return await _context.ParkingReservations
            .Include(r => r.ParkingSlot)
            .FirstOrDefaultAsync(r => r.Id == reservationId);
    }

    public async Task<bool> HasActiveReservationAsync(int bookingId)
    {
        return await _context.ParkingReservations
            .AnyAsync(r => r.BookingId == bookingId && r.IsActive);
    }

    public async Task<bool> IsSlotActivelyReservedAsync(int parkingSlotId)
    {
        return await _context.ParkingReservations
            .AnyAsync(r => r.ParkingSlotId == parkingSlotId && r.IsActive);
    }

    public async Task AddAsync(ParkingReservation reservation)
    {
        await _context.ParkingReservations.AddAsync(reservation);
    }

    public void Update(ParkingReservation reservation)
    {
        _context.ParkingReservations.Update(reservation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
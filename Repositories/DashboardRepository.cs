using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.Dashboards;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _db;

        public DashboardRepository(AppDbContext db)
        {
            _db = db;
        }

        // =========================
        // ADMIN DASHBOARD
        // =========================
        public async Task<AdminDashboardDto> GetAdminSummaryAsync()
        {
            SeatStatus availableSeat =
                Enum.Parse<SeatStatus>("Available", true);

            PaymentStatus completedPayment =
                Enum.Parse<PaymentStatus>("Completed", true);

            return new AdminDashboardDto
            {
                TotalEvents =
                    await _db.Set<Models.Event>()
                        .CountAsync(),

                TotalBookings =
                    await _db.Set<Booking>()
                        .CountAsync(),

                AvailableSeats =
                    await _db.Set<Seat>()
                        .CountAsync(x =>
                            x.Status == availableSeat),

                // Held = temporarily occupied by pending booking
                // Reserved = confirmed after successful payment
                OccupiedParkingSlots =
                    await _db.Set<ParkingSlot>()
                        .CountAsync(x =>
                            x.Status == ParkingSlotStatus.Held ||
                            x.Status == ParkingSlotStatus.Reserved),

                TotalRevenue =
                    await _db.Set<Payment>()
                        .Where(x =>
                            x.Status == completedPayment)
                        .SumAsync(x =>
                            (decimal?)x.Amount) ?? 0m,

                TotalCustomers =
                    await _db.Set<Customer>()
                        .CountAsync()
            };
        }

        // =========================
        // CUSTOMER DASHBOARD
        // =========================
        public async Task<CustomerDashboardDto>
            GetCustomerSummaryAsync(int customerId)
        {
            BookingStatus pending =
                Enum.Parse<BookingStatus>(
                    "Pending",
                    true);

            BookingStatus confirmed =
                Enum.Parse<BookingStatus>(
                    "Confirmed",
                    true);

            PaymentStatus completedPayment =
                Enum.Parse<PaymentStatus>(
                    "Completed",
                    true);

            // Pending + Confirmed bookings
            int upcoming =
                await _db.Set<Booking>()
                    .CountAsync(x =>
                        x.CustomerId == customerId &&
                        (
                            x.Status == pending ||
                            x.Status == confirmed
                        ));

            // Active parking reservations
            int reservedParking =
                await _db.Set<ParkingReservation>()
                    .Where(x => x.IsActive)
                    .Join(
                        _db.Set<Booking>(),
                        parking => parking.BookingId,
                        booking => booking.Id,
                        (parking, booking) =>
                            new
                            {
                                Parking = parking,
                                Booking = booking
                            })
                    .CountAsync(x =>
                        x.Booking.CustomerId ==
                        customerId);

            // Completed payments
            int recentPayments =
                await _db.Set<Payment>()
                    .Include(x => x.Booking)
                    .CountAsync(x =>
                        x.Booking.CustomerId ==
                            customerId &&
                        x.Status ==
                            completedPayment);

            // Unread notifications
            int unread =
                await _db.Set<Notification>()
                    .CountAsync(x =>
                        x.CustomerId ==
                            customerId &&
                        !x.IsRead);

            return new CustomerDashboardDto
            {
                UpcomingBookings = upcoming,
                ReservedParking = reservedParking,
                RecentPayments = recentPayments,
                UnreadNotifications = unread
            };
        }
    }
}
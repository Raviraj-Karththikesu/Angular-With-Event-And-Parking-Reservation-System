using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Customer and authentication
    public DbSet<Customer> Customers => Set<Customer>();

    // Venue and event management
    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<EventCategory> EventCategories => Set<EventCategory>();

    public DbSet<Event> Events => Set<Event>();

    // Seat and parking management
    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<ParkingSlot> ParkingSlots => Set<ParkingSlot>();

    // Booking management
    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();

    public DbSet<ParkingReservation> ParkingReservations
        => Set<ParkingReservation>();

    // Payment and notifications
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically loads all IEntityTypeConfiguration classes
        // from the Configurations folder.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}
using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs
    .ParkingReservations;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system
    .Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class ParkingReservationServiceTests
    {
        private readonly Mock<
            IParkingReservationRepository>
            _repositoryMock;

        public ParkingReservationServiceTests()
        {
            _repositoryMock =
                new Mock<
                    IParkingReservationRepository>();
        }

        private static AppDbContext CreateContext()
        {
            DbContextOptions<AppDbContext> options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(
                        Guid.NewGuid().ToString()
                    )
                    .Options;

            return new AppDbContext(options);
        }

        private ParkingReservationService CreateService(
            AppDbContext context)
        {
            return new ParkingReservationService(
                _repositoryMock.Object,
                context
            );
        }

        private static async Task SeedBookingAsync(
            AppDbContext context,
            int bookingId = 1,
            int eventId = 1)
        {
            context.Bookings.Add(
                new Booking
                {
                    Id = bookingId,
                    BookingNumber = "BK001",
                    CustomerId = 1,
                    EventId = eventId,
                    Status = BookingStatus.Pending,
                    TotalAmount = 0
                }
            );

            await context.SaveChangesAsync();
        }

        private static async Task SeedBookingAndSlotAsync(
            AppDbContext context,
            int bookingEventId = 1,
            int slotEventId = 1,
            ParkingSlotStatus slotStatus =
                ParkingSlotStatus.Available)
        {
            context.Bookings.Add(
                new Booking
                {
                    Id = 1,
                    BookingNumber = "BK001",
                    CustomerId = 1,
                    EventId = bookingEventId,
                    Status = BookingStatus.Pending,
                    TotalAmount = 0
                }
            );

            context.ParkingSlots.Add(
                new ParkingSlot
                {
                    Id = 1,
                    EventId = slotEventId,
                    SlotNumber = "P1",
                    Zone = "A",
                    Fee = 500m,
                    Status = slotStatus
                }
            );

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task
            ReserveParkingAsync_WhenDataIsValid_CreatesReservation()
        {
            using AppDbContext context =
                CreateContext();

            await SeedBookingAndSlotAsync(context);

            _repositoryMock
                .Setup(repository =>
                    repository
                        .HasActiveReservationAsync(1))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(repository =>
                    repository
                        .IsSlotActivelyReservedAsync(1))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(repository =>
                    repository.AddAsync(
                        It.IsAny<
                            ParkingReservation>()
                    ))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            ParkingReservationService service =
                CreateService(context);

            CreateParkingReservationDto dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            ParkingReservationResponseDto result =
                await service.ReserveParkingAsync(
                    dto
                );

            Assert.Equal(1, result.BookingId);
            Assert.Equal(1, result.ParkingSlotId);
            Assert.Equal("P1", result.SlotNumber);
            Assert.Equal("A", result.Zone);
            Assert.Equal(
                500m,
                result.FeeAtReservation
            );
            Assert.True(result.IsActive);

            ParkingSlot slot =
                await context.ParkingSlots
                    .FirstAsync(item =>
                        item.Id == 1
                    );

            Assert.Equal(
                ParkingSlotStatus.Reserved,
                slot.Status
            );

            _repositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<ParkingReservation>(
                            reservation =>
                                reservation.BookingId ==
                                    1 &&
                                reservation
                                    .ParkingSlotId ==
                                    1 &&
                                reservation
                                    .FeeAtReservation ==
                                    500m &&
                                reservation.IsActive
                        )
                    ),
                Times.Once
            );

            _repositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task
            ReserveParkingAsync_WhenBookingDoesNotExist_ThrowsNotFoundException()
        {
            using AppDbContext context =
                CreateContext();

            ParkingReservationService service =
                CreateService(context);

            CreateParkingReservationDto dto =
                new CreateParkingReservationDto
                {
                    BookingId = 999,
                    ParkingSlotId = 1
                };

            NotFoundException exception =
                await Assert.ThrowsAsync<
                    NotFoundException>(
                    () =>
                        service
                            .ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Booking not found.",
                exception.Message
            );

            _repositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.IsAny<
                            ParkingReservation>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task
            ReserveParkingAsync_WhenParkingSlotDoesNotExist_ThrowsNotFoundException()
        {
            using AppDbContext context =
                CreateContext();

            await SeedBookingAsync(context);

            ParkingReservationService service =
                CreateService(context);

            CreateParkingReservationDto dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 999
                };

            NotFoundException exception =
                await Assert.ThrowsAsync<
                    NotFoundException>(
                    () =>
                        service
                            .ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Parking slot not found.",
                exception.Message
            );
        }

        [Fact]
        public async Task
            ReserveParkingAsync_WhenSlotBelongsToDifferentEvent_ThrowsConflictException()
        {
            using AppDbContext context =
                CreateContext();

            await SeedBookingAndSlotAsync(
                context,
                bookingEventId: 1,
                slotEventId: 2
            );

            ParkingReservationService service =
                CreateService(context);

            CreateParkingReservationDto dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            ConflictException exception =
                await Assert.ThrowsAsync<
                    ConflictException>(
                    () =>
                        service
                            .ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Parking slot does not belong " +
                "to the booked event.",
                exception.Message
            );
        }

        [Fact]
        public async Task
            ReserveParkingAsync_WhenBookingAlreadyHasReservation_ThrowsConflictException()
        {
            using AppDbContext context =
                CreateContext();

            await SeedBookingAndSlotAsync(context);

            _repositoryMock
                .Setup(repository =>
                    repository
                        .HasActiveReservationAsync(1))
                .ReturnsAsync(true);

            ParkingReservationService service =
                CreateService(context);

            CreateParkingReservationDto dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            ConflictException exception =
                await Assert.ThrowsAsync<
                    ConflictException>(
                    () =>
                        service
                            .ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "This booking already has an " +
                "active parking reservation.",
                exception.Message
            );
        }

        [Fact]
        public async Task
            ReserveParkingAsync_WhenSlotIsNotAvailable_ThrowsConflictException()
        {
            using AppDbContext context =
                CreateContext();

            await SeedBookingAndSlotAsync(
                context,
                slotStatus:
                    ParkingSlotStatus.Reserved
            );

            _repositoryMock
                .Setup(repository =>
                    repository
                        .HasActiveReservationAsync(1))
                .ReturnsAsync(false);

            ParkingReservationService service =
                CreateService(context);

            CreateParkingReservationDto dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            ConflictException exception =
                await Assert.ThrowsAsync<
                    ConflictException>(
                    () =>
                        service
                            .ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Parking slot is not available.",
                exception.Message
            );
        }

        [Fact]
        public async Task
            ReserveParkingAsync_WhenSlotAlreadyReserved_ThrowsConflictException()
        {
            using AppDbContext context =
                CreateContext();

            await SeedBookingAndSlotAsync(context);

            _repositoryMock
                .Setup(repository =>
                    repository
                        .HasActiveReservationAsync(1))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(repository =>
                    repository
                        .IsSlotActivelyReservedAsync(1))
                .ReturnsAsync(true);

            ParkingReservationService service =
                CreateService(context);

            CreateParkingReservationDto dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            ConflictException exception =
                await Assert.ThrowsAsync<
                    ConflictException>(
                    () =>
                        service
                            .ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Parking slot is already reserved.",
                exception.Message
            );
        }

        [Fact]
        public async Task
            GetByBookingIdAsync_WhenReservationExists_ReturnsReservation()
        {
            using AppDbContext context =
                CreateContext();

            ParkingReservation reservation =
                new ParkingReservation
                {
                    Id = 1,
                    BookingId = 1,
                    ParkingSlotId = 1,
                    FeeAtReservation = 500m,
                    IsActive = true,
                    ReservedAt = DateTime.UtcNow,
                    ParkingSlot =
                        new ParkingSlot
                        {
                            Id = 1,
                            EventId = 1,
                            SlotNumber = "P1",
                            Zone = "A",
                            Fee = 500m,
                            Status =
                                ParkingSlotStatus
                                    .Reserved
                        }
                };

            _repositoryMock
                .Setup(repository =>
                    repository
                        .GetByBookingIdAsync(1))
                .ReturnsAsync(reservation);

            ParkingReservationService service =
                CreateService(context);

            ParkingReservationResponseDto? result =
                await service.GetByBookingIdAsync(
                    1
                );

            Assert.NotNull(result);
            Assert.Equal(1, result.BookingId);
            Assert.Equal(1, result.ParkingSlotId);
            Assert.Equal("P1", result.SlotNumber);
            Assert.Equal("A", result.Zone);
            Assert.Equal(
                500m,
                result.FeeAtReservation
            );
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task
            GetByBookingIdAsync_WhenReservationDoesNotExist_ReturnsNull()
        {
            using AppDbContext context =
                CreateContext();

            _repositoryMock
                .Setup(repository =>
                    repository
                        .GetByBookingIdAsync(999))
                .ReturnsAsync(
                    (ParkingReservation?)null
                );

            ParkingReservationService service =
                CreateService(context);

            ParkingReservationResponseDto? result =
                await service.GetByBookingIdAsync(
                    999
                );

            Assert.Null(result);
        }

        [Fact]
        public async Task
            ReleaseParkingAsync_WhenReservationIsActive_ReleasesParking()
        {
            using AppDbContext context =
                CreateContext();

            ParkingSlot parkingSlot =
                new ParkingSlot
                {
                    Id = 1,
                    EventId = 1,
                    SlotNumber = "P1",
                    Zone = "A",
                    Fee = 500m,
                    Status =
                        ParkingSlotStatus.Reserved
                };

            ParkingReservation reservation =
                new ParkingReservation
                {
                    Id = 1,
                    BookingId = 1,
                    ParkingSlotId = 1,
                    FeeAtReservation = 500m,
                    IsActive = true,
                    ReservedAt = DateTime.UtcNow,
                    ParkingSlot = parkingSlot
                };

            _repositoryMock
                .Setup(repository =>
                    repository
                        .GetByBookingIdAsync(1))
                .ReturnsAsync(reservation);

            _repositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            ParkingReservationService service =
                CreateService(context);

            await service.ReleaseParkingAsync(1);

            Assert.False(reservation.IsActive);
            Assert.NotNull(reservation.ReleasedAt);

            Assert.Equal(
                ParkingSlotStatus.Available,
                parkingSlot.Status
            );

            _repositoryMock.Verify(
                repository =>
                    repository.Update(reservation),
                Times.Once
            );

            _repositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task
            ReleaseParkingAsync_WhenReservationDoesNotExist_ThrowsNotFoundException()
        {
            using AppDbContext context =
                CreateContext();

            _repositoryMock
                .Setup(repository =>
                    repository
                        .GetByBookingIdAsync(999))
                .ReturnsAsync(
                    (ParkingReservation?)null
                );

            ParkingReservationService service =
                CreateService(context);

            NotFoundException exception =
                await Assert.ThrowsAsync<
                    NotFoundException>(
                    () =>
                        service
                            .ReleaseParkingAsync(999)
                );

            Assert.Equal(
                "Active parking reservation " +
                "not found.",
                exception.Message
            );

            _repositoryMock.Verify(
                repository =>
                    repository.Update(
                        It.IsAny<
                            ParkingReservation>()
                    ),
                Times.Never
            );
        }
    }
}
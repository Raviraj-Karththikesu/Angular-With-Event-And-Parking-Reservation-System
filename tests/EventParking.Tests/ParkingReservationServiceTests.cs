using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.ParkingReservations;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class ParkingReservationServiceTests
    {
        private readonly Mock<IParkingReservationRepository>
            _parkingReservationRepositoryMock;

        public ParkingReservationServiceTests()
        {
            _parkingReservationRepositoryMock =
                new Mock<IParkingReservationRepository>();
        }

        private AppDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(
                        Guid.NewGuid().ToString()
                    )
                    .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task ReserveParkingAsync_WhenDataIsValid_CreatesReservation()
        {
            using var context = CreateContext();

            context.Bookings.Add(
                new Booking
                {
                    Id = 1,
                    BookingNumber = "BK001",
                    CustomerId = 1,
                    EventId = 1,
                    Status = BookingStatus.Pending,
                    TotalAmount = 0
                }
            );

            context.ParkingSlots.Add(
                new ParkingSlot
                {
                    Id = 1,
                    EventId = 1,
                    SlotNumber = "P1",
                    Zone = "A",
                    Fee = 500,
                    Status = ParkingSlotStatus.Available
                }
            );

            await context.SaveChangesAsync();

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.HasActiveReservationAsync(1))
                .ReturnsAsync(false);

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.IsSlotActivelyReservedAsync(1))
                .ReturnsAsync(false);

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.AddAsync(
                        It.IsAny<ParkingReservation>()
                    ))
                .Returns(Task.CompletedTask);

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            var dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            ParkingReservationResponseDto result =
                await service.ReserveParkingAsync(dto);

            Assert.Equal(1, result.BookingId);
            Assert.Equal(1, result.ParkingSlotId);
            Assert.Equal("P1", result.SlotNumber);
            Assert.Equal("A", result.Zone);
            Assert.Equal(500, result.FeeAtReservation);
            Assert.True(result.IsActive);

            ParkingSlot slot =
                await context.ParkingSlots
                    .FirstAsync(p => p.Id == 1);

            Assert.Equal(
                ParkingSlotStatus.Reserved,
                slot.Status
            );

            _parkingReservationRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.Is<ParkingReservation>(
                            reservation =>
                                reservation.BookingId == 1 &&
                                reservation.ParkingSlotId == 1 &&
                                reservation.FeeAtReservation == 500 &&
                                reservation.IsActive
                        )
                    ),
                Times.Once
            );

            _parkingReservationRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task ReserveParkingAsync_WhenBookingDoesNotExist_ThrowsKeyNotFoundException()
        {
            using var context = CreateContext();

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            var dto =
                new CreateParkingReservationDto
                {
                    BookingId = 999,
                    ParkingSlotId = 1
                };

            KeyNotFoundException exception =
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () =>
                        service.ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Booking not found.",
                exception.Message
            );

            _parkingReservationRepositoryMock.Verify(
                repository =>
                    repository.AddAsync(
                        It.IsAny<ParkingReservation>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task ReserveParkingAsync_WhenParkingSlotDoesNotExist_ThrowsKeyNotFoundException()
        {
            using var context = CreateContext();

            context.Bookings.Add(
                new Booking
                {
                    Id = 1,
                    BookingNumber = "BK001",
                    CustomerId = 1,
                    EventId = 1,
                    Status = BookingStatus.Pending,
                    TotalAmount = 0
                }
            );

            await context.SaveChangesAsync();

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            var dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 999
                };

            KeyNotFoundException exception =
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () =>
                        service.ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Parking slot not found.",
                exception.Message
            );
        }

        [Fact]
        public async Task ReserveParkingAsync_WhenSlotBelongsToDifferentEvent_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            context.Bookings.Add(
                new Booking
                {
                    Id = 1,
                    BookingNumber = "BK001",
                    CustomerId = 1,
                    EventId = 1,
                    Status = BookingStatus.Pending,
                    TotalAmount = 0
                }
            );

            context.ParkingSlots.Add(
                new ParkingSlot
                {
                    Id = 1,
                    EventId = 2,
                    SlotNumber = "P1",
                    Zone = "A",
                    Fee = 500,
                    Status = ParkingSlotStatus.Available
                }
            );

            await context.SaveChangesAsync();

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            var dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () =>
                        service.ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Parking slot does not belong to the booked event.",
                exception.Message
            );
        }

        [Fact]
        public async Task ReserveParkingAsync_WhenBookingAlreadyHasReservation_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            context.Bookings.Add(
                new Booking
                {
                    Id = 1,
                    BookingNumber = "BK001",
                    CustomerId = 1,
                    EventId = 1,
                    Status = BookingStatus.Pending,
                    TotalAmount = 0
                }
            );

            context.ParkingSlots.Add(
                new ParkingSlot
                {
                    Id = 1,
                    EventId = 1,
                    SlotNumber = "P1",
                    Zone = "A",
                    Fee = 500,
                    Status = ParkingSlotStatus.Available
                }
            );

            await context.SaveChangesAsync();

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.HasActiveReservationAsync(1))
                .ReturnsAsync(true);

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            var dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () =>
                        service.ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "This booking already has an active parking reservation.",
                exception.Message
            );
        }

        [Fact]
        public async Task ReserveParkingAsync_WhenSlotIsNotAvailable_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            context.Bookings.Add(
                new Booking
                {
                    Id = 1,
                    BookingNumber = "BK001",
                    CustomerId = 1,
                    EventId = 1,
                    Status = BookingStatus.Pending,
                    TotalAmount = 0
                }
            );

            context.ParkingSlots.Add(
                new ParkingSlot
                {
                    Id = 1,
                    EventId = 1,
                    SlotNumber = "P1",
                    Zone = "A",
                    Fee = 500,
                    Status = ParkingSlotStatus.Reserved
                }
            );

            await context.SaveChangesAsync();

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.HasActiveReservationAsync(1))
                .ReturnsAsync(false);

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            var dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () =>
                        service.ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Parking slot is not available.",
                exception.Message
            );
        }

        [Fact]
        public async Task ReserveParkingAsync_WhenSlotAlreadyHasActiveReservation_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            context.Bookings.Add(
                new Booking
                {
                    Id = 1,
                    BookingNumber = "BK001",
                    CustomerId = 1,
                    EventId = 1,
                    Status = BookingStatus.Pending,
                    TotalAmount = 0
                }
            );

            context.ParkingSlots.Add(
                new ParkingSlot
                {
                    Id = 1,
                    EventId = 1,
                    SlotNumber = "P1",
                    Zone = "A",
                    Fee = 500,
                    Status = ParkingSlotStatus.Available
                }
            );

            await context.SaveChangesAsync();

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.HasActiveReservationAsync(1))
                .ReturnsAsync(false);

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.IsSlotActivelyReservedAsync(1))
                .ReturnsAsync(true);

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            var dto =
                new CreateParkingReservationDto
                {
                    BookingId = 1,
                    ParkingSlotId = 1
                };

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () =>
                        service.ReserveParkingAsync(dto)
                );

            Assert.Equal(
                "Parking slot is already reserved.",
                exception.Message
            );
        }

        [Fact]
        public async Task GetByBookingIdAsync_WhenReservationExists_ReturnsReservation()
        {
            using var context = CreateContext();

            var reservation =
                new ParkingReservation
                {
                    Id = 1,
                    BookingId = 1,
                    ParkingSlotId = 1,
                    FeeAtReservation = 500,
                    IsActive = true,
                    ReservedAt = DateTime.UtcNow,
                    ParkingSlot = new ParkingSlot
                    {
                        Id = 1,
                        EventId = 1,
                        SlotNumber = "P1",
                        Zone = "A",
                        Fee = 500,
                        Status = ParkingSlotStatus.Reserved
                    }
                };

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.GetByBookingIdAsync(1))
                .ReturnsAsync(reservation);

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            ParkingReservationResponseDto? result =
                await service.GetByBookingIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.BookingId);
            Assert.Equal(1, result.ParkingSlotId);
            Assert.Equal("P1", result.SlotNumber);
            Assert.Equal("A", result.Zone);
            Assert.Equal(500, result.FeeAtReservation);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetByBookingIdAsync_WhenReservationDoesNotExist_ReturnsNull()
        {
            using var context = CreateContext();

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.GetByBookingIdAsync(999))
                .ReturnsAsync((ParkingReservation?)null);

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            ParkingReservationResponseDto? result =
                await service.GetByBookingIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task ReleaseParkingAsync_WhenReservationIsActive_ReleasesParking()
        {
            using var context = CreateContext();

            var parkingSlot =
                new ParkingSlot
                {
                    Id = 1,
                    EventId = 1,
                    SlotNumber = "P1",
                    Zone = "A",
                    Fee = 500,
                    Status = ParkingSlotStatus.Reserved
                };

            var reservation =
                new ParkingReservation
                {
                    Id = 1,
                    BookingId = 1,
                    ParkingSlotId = 1,
                    FeeAtReservation = 500,
                    IsActive = true,
                    ReservedAt = DateTime.UtcNow,
                    ParkingSlot = parkingSlot
                };

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.GetByBookingIdAsync(1))
                .ReturnsAsync(reservation);

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            await service.ReleaseParkingAsync(1);

            Assert.False(reservation.IsActive);
            Assert.NotNull(reservation.ReleasedAt);

            Assert.Equal(
                ParkingSlotStatus.Available,
                parkingSlot.Status
            );

            _parkingReservationRepositoryMock.Verify(
                repository =>
                    repository.Update(reservation),
                Times.Once
            );

            _parkingReservationRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task ReleaseParkingAsync_WhenActiveReservationDoesNotExist_ThrowsKeyNotFoundException()
        {
            using var context = CreateContext();

            _parkingReservationRepositoryMock
                .Setup(repository =>
                    repository.GetByBookingIdAsync(999))
                .ReturnsAsync((ParkingReservation?)null);

            var service =
                new ParkingReservationService(
                    _parkingReservationRepositoryMock.Object,
                    context
                );

            KeyNotFoundException exception =
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () =>
                        service.ReleaseParkingAsync(999)
                );

            Assert.Equal(
                "Active parking reservation not found.",
                exception.Message
            );

            _parkingReservationRepositoryMock.Verify(
                repository =>
                    repository.Update(
                        It.IsAny<ParkingReservation>()
                    ),
                Times.Never
            );
        }
    }
}
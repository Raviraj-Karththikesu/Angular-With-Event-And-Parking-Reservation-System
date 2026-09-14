using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.BookingSeats;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class BookingSeatServiceTests
    {
        private readonly Mock<IBookingSeatRepository>
            _bookingSeatRepositoryMock;

        public BookingSeatServiceTests()
        {
            _bookingSeatRepositoryMock =
                new Mock<IBookingSeatRepository>();
        }

        private AppDbContext CreateContext()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(
                        Guid.NewGuid().ToString()
                    )
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(
                            InMemoryEventId.TransactionIgnoredWarning
                        )
                    )
                    .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddSeatsToBookingAsync_WhenDataIsValid_AddsSeats()
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

            context.Seats.AddRange(
                new Seat
                {
                    Id = 1,
                    EventId = 1,
                    SeatNumber = "A1",
                    RowLabel = "A",
                    SeatType = "Standard",
                    Price = 1000,
                    Status = SeatStatus.Available
                },
                new Seat
                {
                    Id = 2,
                    EventId = 1,
                    SeatNumber = "A2",
                    RowLabel = "A",
                    SeatType = "Standard",
                    Price = 1500,
                    Status = SeatStatus.Available
                }
            );

            await context.SaveChangesAsync();

            _bookingSeatRepositoryMock
                .Setup(repository =>
                    repository.HasActiveSeatAsync(
                        It.IsAny<int>()
                    ))
                .ReturnsAsync(false);

            _bookingSeatRepositoryMock
                .Setup(repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<BookingSeat>>()
                    ))
                .Returns(Task.CompletedTask);

            _bookingSeatRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service =
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            var dto =
                new AddBookingSeatsDto
                {
                    SeatIds = new List<int>
                    {
                        1,
                        2
                    }
                };

            List<BookingSeatResponseDto> result =
                await service.AddSeatsToBookingAsync(
                    1,
                    dto
                );

            Assert.Equal(2, result.Count);

            Assert.Equal(1, result[0].SeatId);
            Assert.Equal("A1", result[0].SeatNumber);
            Assert.Equal(1000, result[0].PriceAtBooking);
            Assert.True(result[0].IsActive);

            Assert.Equal(2, result[1].SeatId);
            Assert.Equal("A2", result[1].SeatNumber);
            Assert.Equal(1500, result[1].PriceAtBooking);

            var seats =
                await context.Seats
                    .OrderBy(seat => seat.Id)
                    .ToListAsync();

            Assert.All(
                seats,
                seat =>
                    Assert.Equal(
                        SeatStatus.Booked,
                        seat.Status
                    )
            );

            _bookingSeatRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.Is<IEnumerable<BookingSeat>>(
                            bookingSeats =>
                                bookingSeats.Count() == 2
                        )
                    ),
                Times.Once
            );

            _bookingSeatRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task AddSeatsToBookingAsync_WhenSeatIdsContainDuplicates_ThrowsConflictException()
        {
            using var context = CreateContext();

            var service =
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            var dto =
                new AddBookingSeatsDto
                {
                    SeatIds = new List<int>
                    {
                        1,
                        1
                    }
                };

            ConflictException exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () =>
                        service.AddSeatsToBookingAsync(
                            1,
                            dto
                        )
                );

            Assert.Equal(
                "The same seat cannot be selected more than once.",
                exception.Message
            );

            _bookingSeatRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<BookingSeat>>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task AddSeatsToBookingAsync_WhenBookingDoesNotExist_ThrowsNotFoundException()
        {
            using var context = CreateContext();

            var service =
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            var dto =
                new AddBookingSeatsDto
                {
                    SeatIds = new List<int>
                    {
                        1
                    }
                };

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () =>
                        service.AddSeatsToBookingAsync(
                            999,
                            dto
                        )
                );

            Assert.Equal(
                "Booking not found.",
                exception.Message
            );

            _bookingSeatRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<BookingSeat>>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task AddSeatsToBookingAsync_WhenSeatDoesNotExist_ThrowsNotFoundException()
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
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            var dto =
                new AddBookingSeatsDto
                {
                    SeatIds = new List<int>
                    {
                        999
                    }
                };

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () =>
                        service.AddSeatsToBookingAsync(
                            1,
                            dto
                        )
                );

            Assert.Equal(
                "One or more selected seats were not found.",
                exception.Message
            );
        }

        [Fact]
        public async Task AddSeatsToBookingAsync_WhenSeatBelongsToDifferentEvent_ThrowsConflictException()
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

            context.Seats.Add(
                new Seat
                {
                    Id = 1,
                    EventId = 2,
                    SeatNumber = "A1",
                    RowLabel = "A",
                    SeatType = "Standard",
                    Price = 1000,
                    Status = SeatStatus.Available
                }
            );

            await context.SaveChangesAsync();

            var service =
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            var dto =
                new AddBookingSeatsDto
                {
                    SeatIds = new List<int>
                    {
                        1
                    }
                };

            ConflictException exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () =>
                        service.AddSeatsToBookingAsync(
                            1,
                            dto
                        )
                );

            Assert.Equal(
                "One or more seats do not belong to the booking event.",
                exception.Message
            );
        }

        [Fact]
        public async Task AddSeatsToBookingAsync_WhenSeatIsAlreadyBooked_ThrowsConflictException()
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

            context.Seats.Add(
                new Seat
                {
                    Id = 1,
                    EventId = 1,
                    SeatNumber = "A1",
                    RowLabel = "A",
                    SeatType = "Standard",
                    Price = 1000,
                    Status = SeatStatus.Booked
                }
            );

            await context.SaveChangesAsync();

            _bookingSeatRepositoryMock
                .Setup(repository =>
                    repository.HasActiveSeatAsync(1))
                .ReturnsAsync(false);

            var service =
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            var dto =
                new AddBookingSeatsDto
                {
                    SeatIds = new List<int>
                    {
                        1
                    }
                };

            ConflictException exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () =>
                        service.AddSeatsToBookingAsync(
                            1,
                            dto
                        )
                );

            Assert.Equal(
                "One or more seats are already booked: 1",
                exception.Message
            );

            _bookingSeatRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<BookingSeat>>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task AddSeatsToBookingAsync_WhenActiveReservationExists_ThrowsConflictException()
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

            context.Seats.Add(
                new Seat
                {
                    Id = 1,
                    EventId = 1,
                    SeatNumber = "A1",
                    RowLabel = "A",
                    SeatType = "Standard",
                    Price = 1000,
                    Status = SeatStatus.Available
                }
            );

            await context.SaveChangesAsync();

            _bookingSeatRepositoryMock
                .Setup(repository =>
                    repository.HasActiveSeatAsync(1))
                .ReturnsAsync(true);

            var service =
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            var dto =
                new AddBookingSeatsDto
                {
                    SeatIds = new List<int>
                    {
                        1
                    }
                };

            ConflictException exception =
                await Assert.ThrowsAsync<ConflictException>(
                    () =>
                        service.AddSeatsToBookingAsync(
                            1,
                            dto
                        )
                );

            Assert.Equal(
                "One or more seats are already booked: 1",
                exception.Message
            );
        }

        [Fact]
        public async Task GetBookingSeatsAsync_WhenBookingExists_ReturnsSeats()
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

            var bookingSeats =
                new List<BookingSeat>
                {
                    new BookingSeat
                    {
                        Id = 1,
                        BookingId = 1,
                        SeatId = 1,
                        PriceAtBooking = 1000,
                        IsActive = true,
                        ReservedAt = DateTime.UtcNow,
                        Seat = new Seat
                        {
                            Id = 1,
                            EventId = 1,
                            SeatNumber = "A1",
                            RowLabel = "A",
                            SeatType = "Standard",
                            Price = 1000,
                            Status = SeatStatus.Booked
                        }
                    }
                };

            _bookingSeatRepositoryMock
                .Setup(repository =>
                    repository.GetByBookingIdAsync(1))
                .ReturnsAsync(bookingSeats);

            var service =
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            List<BookingSeatResponseDto> result =
                await service.GetBookingSeatsAsync(1);

            Assert.Single(result);
            Assert.Equal(1, result[0].BookingId);
            Assert.Equal(1, result[0].SeatId);
            Assert.Equal("A1", result[0].SeatNumber);
            Assert.Equal(1000, result[0].PriceAtBooking);
            Assert.True(result[0].IsActive);

            _bookingSeatRepositoryMock.Verify(
                repository =>
                    repository.GetByBookingIdAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task GetBookingSeatsAsync_WhenBookingDoesNotExist_ThrowsNotFoundException()
        {
            using var context = CreateContext();

            var service =
                new BookingSeatService(
                    _bookingSeatRepositoryMock.Object,
                    context
                );

            NotFoundException exception =
                await Assert.ThrowsAsync<NotFoundException>(
                    () =>
                        service.GetBookingSeatsAsync(999)
                );

            Assert.Equal(
                "Booking not found.",
                exception.Message
            );

            _bookingSeatRepositoryMock.Verify(
                repository =>
                    repository.GetByBookingIdAsync(
                        It.IsAny<int>()
                    ),
                Times.Never
            );
        }
    }
}
using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.Seats;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class SeatServiceTests
    {
        private readonly Mock<ISeatRepository> _seatRepositoryMock;

        public SeatServiceTests()
        {
            _seatRepositoryMock = new Mock<ISeatRepository>();
        }

        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetSeatMapAsync_WhenEventExists_ReturnsSeats()
        {
            using var context = CreateContext();

            context.Events.Add(new Event
            {
                Id = 1,
                Name = "Test Event",
                Capacity = 2,
                TicketPrice = 1000
            });

            await context.SaveChangesAsync();

            var seats = new List<Seat>
            {
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
                    Price = 1000,
                    Status = SeatStatus.Available
                }
            };

            _seatRepositoryMock
                .Setup(repository =>
                    repository.GetByEventIdAsync(1))
                .ReturnsAsync(seats);

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            List<SeatResponseDto> result =
                await service.GetSeatMapAsync(1);

            Assert.Equal(2, result.Count);
            Assert.Equal("A1", result[0].SeatNumber);
            Assert.Equal("A2", result[1].SeatNumber);

            _seatRepositoryMock.Verify(
                repository =>
                    repository.GetByEventIdAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task GetSeatMapAsync_WhenEventDoesNotExist_ThrowsKeyNotFoundException()
        {
            using var context = CreateContext();

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            KeyNotFoundException exception =
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => service.GetSeatMapAsync(999)
                );

            Assert.Equal(
                "Event not found.",
                exception.Message
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.GetByEventIdAsync(
                        It.IsAny<int>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task GenerateSeatMapAsync_WhenDataIsValid_CreatesSeats()
        {
            using var context = CreateContext();

            context.Events.Add(new Event
            {
                Id = 1,
                Name = "Test Event",
                Capacity = 4,
                TicketPrice = 1500
            });

            await context.SaveChangesAsync();

            var dto = new GenerateSeatMapDto
            {
                Rows = 2,
                Columns = 2,
                SeatType = "Standard",
                Price = 2000
            };

            _seatRepositoryMock
                .Setup(repository =>
                    repository.SeatMapExistsAsync(1))
                .ReturnsAsync(false);

            _seatRepositoryMock
                .Setup(repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<Seat>>()
                    ))
                .Returns(Task.CompletedTask);

            _seatRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            List<SeatResponseDto> result =
                await service.GenerateSeatMapAsync(
                    1,
                    dto
                );

            Assert.Equal(4, result.Count);
            Assert.Equal("A1", result[0].SeatNumber);
            Assert.Equal("A2", result[1].SeatNumber);
            Assert.Equal("B1", result[2].SeatNumber);
            Assert.Equal("B2", result[3].SeatNumber);

            Assert.All(
                result,
                seat =>
                    Assert.Equal(2000, seat.Price)
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.Is<IEnumerable<Seat>>(seats =>
                            seats.Count() == 4
                        )
                    ),
                Times.Once
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateSeatMapAsync_WhenSeatCountDoesNotMatchCapacity_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            context.Events.Add(new Event
            {
                Id = 1,
                Name = "Test Event",
                Capacity = 10,
                TicketPrice = 1500
            });

            await context.SaveChangesAsync();

            var dto = new GenerateSeatMapDto
            {
                Rows = 2,
                Columns = 2,
                SeatType = "Standard",
                Price = 1000
            };

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.GenerateSeatMapAsync(
                        1,
                        dto
                    )
                );

            Assert.Equal(
                "Seat count must exactly match event capacity of 10.",
                exception.Message
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<Seat>>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task GenerateSeatMapAsync_WhenSeatMapAlreadyExists_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            context.Events.Add(new Event
            {
                Id = 1,
                Name = "Test Event",
                Capacity = 4,
                TicketPrice = 1500
            });

            await context.SaveChangesAsync();

            var dto = new GenerateSeatMapDto
            {
                Rows = 2,
                Columns = 2,
                SeatType = "Standard",
                Price = 1000
            };

            _seatRepositoryMock
                .Setup(repository =>
                    repository.SeatMapExistsAsync(1))
                .ReturnsAsync(true);

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.GenerateSeatMapAsync(
                        1,
                        dto
                    )
                );

            Assert.Equal(
                "Seat map already exists for this event.",
                exception.Message
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<Seat>>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task UpdateSeatAsync_WhenSeatExists_UpdatesSeat()
        {
            using var context = CreateContext();

            var seat = new Seat
            {
                Id = 1,
                EventId = 1,
                SeatNumber = "A1",
                RowLabel = "A",
                SeatType = "Standard",
                Price = 1000,
                Status = SeatStatus.Available,
                BookingSeats = new List<BookingSeat>()
            };

            _seatRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(seat);

            _seatRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var dto = new UpdateSeatDto
            {
                SeatNumber = "VIP1",
                RowLabel = "VIP",
                SeatType = "VIP",
                Price = 5000
            };

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            SeatResponseDto result =
                await service.UpdateSeatAsync(
                    1,
                    1,
                    dto
                );

            Assert.Equal("VIP1", result.SeatNumber);
            Assert.Equal("VIP", result.RowLabel);
            Assert.Equal("VIP", result.SeatType);
            Assert.Equal(5000, result.Price);

            _seatRepositoryMock.Verify(
                repository =>
                    repository.Update(seat),
                Times.Once
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateSeatAsync_WhenSeatIsBooked_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            var seat = new Seat
            {
                Id = 1,
                EventId = 1,
                SeatNumber = "A1",
                RowLabel = "A",
                SeatType = "Standard",
                Price = 1000,
                Status = SeatStatus.Booked,
                BookingSeats = new List<BookingSeat>
                {
                    new BookingSeat
                    {
                        Id = 1,
                        BookingId = 1,
                        SeatId = 1
                    }
                }
            };

            _seatRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(seat);

            var dto = new UpdateSeatDto
            {
                SeatNumber = "A1",
                RowLabel = "A",
                SeatType = "VIP",
                Price = 2000
            };

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.UpdateSeatAsync(
                        1,
                        1,
                        dto
                    )
                );

            Assert.Equal(
                "A booked seat cannot be edited.",
                exception.Message
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.Update(
                        It.IsAny<Seat>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task DeleteSeatAsync_WhenSeatExists_DeletesSeat()
        {
            using var context = CreateContext();

            var seat = new Seat
            {
                Id = 1,
                EventId = 1,
                SeatNumber = "A1",
                RowLabel = "A",
                SeatType = "Standard",
                Price = 1000,
                Status = SeatStatus.Available,
                BookingSeats = new List<BookingSeat>()
            };

            _seatRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(seat);

            _seatRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            await service.DeleteSeatAsync(1, 1);

            _seatRepositoryMock.Verify(
                repository =>
                    repository.Remove(seat),
                Times.Once
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteSeatAsync_WhenSeatIsBooked_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            var seat = new Seat
            {
                Id = 1,
                EventId = 1,
                SeatNumber = "A1",
                RowLabel = "A",
                SeatType = "Standard",
                Price = 1000,
                Status = SeatStatus.Booked,
                BookingSeats = new List<BookingSeat>
                {
                    new BookingSeat
                    {
                        Id = 1,
                        BookingId = 1,
                        SeatId = 1
                    }
                }
            };

            _seatRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(seat);

            var service = new SeatService(
                _seatRepositoryMock.Object,
                context
            );

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.DeleteSeatAsync(
                        1,
                        1
                    )
                );

            Assert.Equal(
                "A booked seat cannot be deleted.",
                exception.Message
            );

            _seatRepositoryMock.Verify(
                repository =>
                    repository.Remove(
                        It.IsAny<Seat>()
                    ),
                Times.Never
            );
        }
    }
}
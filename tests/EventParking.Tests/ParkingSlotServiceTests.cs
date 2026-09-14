using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.DTOs.Parking;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Models;
using Event_and_parking_reservation_system.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EventParking.Tests
{
    public class ParkingSlotServiceTests
    {
        private readonly Mock<IParkingSlotRepository>
            _parkingSlotRepositoryMock;

        public ParkingSlotServiceTests()
        {
            _parkingSlotRepositoryMock =
                new Mock<IParkingSlotRepository>();
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
        public async Task GetParkingLayoutAsync_WhenEventExists_ReturnsParkingSlots()
        {
            using var context = CreateContext();

            context.Events.Add(
                new Event
                {
                    Id = 1,
                    Name = "Test Event",
                    Capacity = 100,
                    TicketPrice = 1000,
                    ParkingFee = 500
                }
            );

            await context.SaveChangesAsync();

            var parkingSlots =
                new List<ParkingSlot>
                {
                    new ParkingSlot
                    {
                        Id = 1,
                        EventId = 1,
                        SlotNumber = "P1",
                        Zone = "A",
                        Fee = 500,
                        Status = ParkingSlotStatus.Available
                    },
                    new ParkingSlot
                    {
                        Id = 2,
                        EventId = 1,
                        SlotNumber = "P2",
                        Zone = "A",
                        Fee = 500,
                        Status = ParkingSlotStatus.Available
                    }
                };

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.GetByEventIdAsync(1))
                .ReturnsAsync(parkingSlots);

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            List<ParkingSlotResponseDto> result =
                await service.GetParkingLayoutAsync(1);

            Assert.Equal(2, result.Count);
            Assert.Equal("P1", result[0].SlotNumber);
            Assert.Equal("P2", result[1].SlotNumber);

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.GetByEventIdAsync(1),
                Times.Once
            );
        }

        [Fact]
        public async Task GetParkingLayoutAsync_WhenEventDoesNotExist_ThrowsKeyNotFoundException()
        {
            using var context = CreateContext();

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            KeyNotFoundException exception =
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () =>
                        service.GetParkingLayoutAsync(999)
                );

            Assert.Equal(
                "Event not found.",
                exception.Message
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.GetByEventIdAsync(
                        It.IsAny<int>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task GenerateParkingLayoutAsync_WhenDataIsValid_CreatesParkingSlots()
        {
            using var context = CreateContext();

            context.Events.Add(
                new Event
                {
                    Id = 1,
                    Name = "Test Event",
                    Capacity = 100,
                    TicketPrice = 1000,
                    ParkingFee = 500
                }
            );

            await context.SaveChangesAsync();

            var dto =
                new GenerateParkingLayoutDto
                {
                    TotalSlots = 3,
                    Zone = "A",
                    Fee = 750
                };

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.ParkingLayoutExistsAsync(1))
                .ReturnsAsync(false);

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<ParkingSlot>>()
                    ))
                .Returns(Task.CompletedTask);

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            List<ParkingSlotResponseDto> result =
                await service.GenerateParkingLayoutAsync(
                    1,
                    dto
                );

            Assert.Equal(3, result.Count);
            Assert.Equal("P1", result[0].SlotNumber);
            Assert.Equal("P2", result[1].SlotNumber);
            Assert.Equal("P3", result[2].SlotNumber);

            Assert.All(
                result,
                slot =>
                    Assert.Equal(750, slot.Fee)
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.Is<IEnumerable<ParkingSlot>>(
                            slots =>
                                slots.Count() == 3
                        )
                    ),
                Times.Once
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task GenerateParkingLayoutAsync_WhenFeeIsZero_UsesEventParkingFee()
        {
            using var context = CreateContext();

            context.Events.Add(
                new Event
                {
                    Id = 1,
                    Name = "Test Event",
                    Capacity = 100,
                    TicketPrice = 1000,
                    ParkingFee = 600
                }
            );

            await context.SaveChangesAsync();

            var dto =
                new GenerateParkingLayoutDto
                {
                    TotalSlots = 2,
                    Zone = "B",
                    Fee = 0
                };

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.ParkingLayoutExistsAsync(1))
                .ReturnsAsync(false);

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<ParkingSlot>>()
                    ))
                .Returns(Task.CompletedTask);

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            List<ParkingSlotResponseDto> result =
                await service.GenerateParkingLayoutAsync(
                    1,
                    dto
                );

            Assert.Equal(2, result.Count);

            Assert.All(
                result,
                slot =>
                    Assert.Equal(600, slot.Fee)
            );
        }

        [Fact]
        public async Task GenerateParkingLayoutAsync_WhenLayoutAlreadyExists_ThrowsInvalidOperationException()
        {
            using var context = CreateContext();

            context.Events.Add(
                new Event
                {
                    Id = 1,
                    Name = "Test Event",
                    Capacity = 100,
                    TicketPrice = 1000,
                    ParkingFee = 500
                }
            );

            await context.SaveChangesAsync();

            var dto =
                new GenerateParkingLayoutDto
                {
                    TotalSlots = 2,
                    Zone = "A",
                    Fee = 500
                };

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.ParkingLayoutExistsAsync(1))
                .ReturnsAsync(true);

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () =>
                        service.GenerateParkingLayoutAsync(
                            1,
                            dto
                        )
                );

            Assert.Equal(
                "Parking layout already exists for this event.",
                exception.Message
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.AddRangeAsync(
                        It.IsAny<IEnumerable<ParkingSlot>>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task UpdateParkingSlotAsync_WhenSlotExists_UpdatesSlot()
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
                    Status = ParkingSlotStatus.Available,
                    ParkingReservations =
                        new List<ParkingReservation>()
                };

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(parkingSlot);

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var dto =
                new UpdateParkingSlotDto
                {
                    SlotNumber = "VIP1",
                    Zone = "VIP",
                    Fee = 1500
                };

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            ParkingSlotResponseDto result =
                await service.UpdateParkingSlotAsync(
                    1,
                    1,
                    dto
                );

            Assert.Equal("VIP1", result.SlotNumber);
            Assert.Equal("VIP", result.Zone);
            Assert.Equal(1500, result.Fee);

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.Update(parkingSlot),
                Times.Once
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateParkingSlotAsync_WhenActiveReservationExists_ThrowsInvalidOperationException()
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
                    Status = ParkingSlotStatus.Reserved,
                    ParkingReservations =
                        new List<ParkingReservation>
                        {
                            new ParkingReservation
                            {
                                Id = 1,
                                BookingId = 1,
                                ParkingSlotId = 1,
                                IsActive = true
                            }
                        }
                };

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(parkingSlot);

            var dto =
                new UpdateParkingSlotDto
                {
                    SlotNumber = "P1",
                    Zone = "B",
                    Fee = 750
                };

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () =>
                        service.UpdateParkingSlotAsync(
                            1,
                            1,
                            dto
                        )
                );

            Assert.Equal(
                "A parking slot with an active reservation cannot be edited.",
                exception.Message
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.Update(
                        It.IsAny<ParkingSlot>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task DeleteParkingSlotAsync_WhenSlotExists_DeletesSlot()
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
                    Status = ParkingSlotStatus.Available,
                    ParkingReservations =
                        new List<ParkingReservation>()
                };

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(parkingSlot);

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            await service.DeleteParkingSlotAsync(
                1,
                1
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.Remove(parkingSlot),
                Times.Once
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.SaveChangesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteParkingSlotAsync_WhenActiveReservationExists_ThrowsInvalidOperationException()
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
                    Status = ParkingSlotStatus.Reserved,
                    ParkingReservations =
                        new List<ParkingReservation>
                        {
                            new ParkingReservation
                            {
                                Id = 1,
                                BookingId = 1,
                                ParkingSlotId = 1,
                                IsActive = true
                            }
                        }
                };

            _parkingSlotRepositoryMock
                .Setup(repository =>
                    repository.GetByIdAsync(1))
                .ReturnsAsync(parkingSlot);

            var service =
                new ParkingSlotService(
                    _parkingSlotRepositoryMock.Object,
                    context
                );

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () =>
                        service.DeleteParkingSlotAsync(
                            1,
                            1
                        )
                );

            Assert.Equal(
                "A parking slot with an active reservation cannot be deleted.",
                exception.Message
            );

            _parkingSlotRepositoryMock.Verify(
                repository =>
                    repository.Remove(
                        It.IsAny<ParkingSlot>()
                    ),
                Times.Never
            );
        }
    }
}
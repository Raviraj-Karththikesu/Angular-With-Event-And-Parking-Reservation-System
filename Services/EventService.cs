using Event_and_parking_reservation_system.DTOs.Events;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IVenueRepository _venueRepository;
        private readonly ICategoryRepository _categoryRepository;

        public EventService(
            IEventRepository eventRepository,
            IVenueRepository venueRepository,
            ICategoryRepository categoryRepository)
        {
            _eventRepository = eventRepository;
            _venueRepository = venueRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<EventResponseDto>> GetAllAsync(
            EventFilterDto filter)
        {
            var events = await _eventRepository.GetAllAsync(filter);

            return events.Select(MapToResponseDto);
        }

        public async Task<EventResponseDto> GetByIdAsync(int id)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);

            if (eventEntity is null)
            {
                throw new NotFoundException("Event was not found.");
            }

            return MapToResponseDto(eventEntity);
        }

        public async Task<EventResponseDto> CreateAsync(CreateEventDto dto)
        {
            if (dto.EndDateTime <= dto.StartDateTime)
            {
                throw new ArgumentException(
                    "End date/time must be later than start date/time.");
            }

            var venue = await _venueRepository.GetByIdAsync(dto.VenueId);

            if (venue is null)
            {
                throw new NotFoundException("Venue was not found.");
            }

            var category =
                await _categoryRepository.GetByIdAsync(dto.EventCategoryId);

            if (category is null)
            {
                throw new NotFoundException("Category was not found.");
            }

            if (dto.Capacity > venue.TotalCapacity)
            {
                throw new ConflictException(
                    "Event capacity cannot exceed venue capacity.");
            }

            var hasOverlap = await _eventRepository.HasOverlapAsync(
                dto.VenueId,
                dto.StartDateTime,
                dto.EndDateTime);

            if (hasOverlap)
            {
                throw new ConflictException(
                    "The selected venue already has an overlapping event.");
            }

            var eventEntity = new Event
            {
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),
                VenueId = dto.VenueId,
                EventCategoryId = dto.EventCategoryId,
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                TicketPrice = dto.TicketPrice,
                ParkingFee = dto.ParkingFee,
                Capacity = dto.Capacity,
                CreatedAt = DateTime.UtcNow
            };

            await _eventRepository.AddAsync(eventEntity);

            eventEntity.Venue = venue;
            eventEntity.EventCategory = category;

            return MapToResponseDto(eventEntity);
        }

        public async Task<EventResponseDto> UpdateAsync(
            int id,
            UpdateEventDto dto)
        {
            if (dto.EndDateTime <= dto.StartDateTime)
            {
                throw new ArgumentException(
                    "End date/time must be later than start date/time.");
            }

            var eventEntity = await _eventRepository.GetByIdAsync(id);

            if (eventEntity is null)
            {
                throw new NotFoundException("Event was not found.");
            }

            var venue = await _venueRepository.GetByIdAsync(dto.VenueId);

            if (venue is null)
            {
                throw new NotFoundException("Venue was not found.");
            }

            var category =
                await _categoryRepository.GetByIdAsync(dto.EventCategoryId);

            if (category is null)
            {
                throw new NotFoundException("Category was not found.");
            }

            if (dto.Capacity > venue.TotalCapacity)
            {
                throw new ConflictException(
                    "Event capacity cannot exceed venue capacity.");
            }

            var bookedSeatCount =
                await _eventRepository.GetBookedSeatCountAsync(id);

            if (dto.Capacity < bookedSeatCount)
            {
                throw new ConflictException(
                    "Event capacity cannot be lower than the number of booked seats.");
            }

            var hasOverlap = await _eventRepository.HasOverlapAsync(
                dto.VenueId,
                dto.StartDateTime,
                dto.EndDateTime,
                id);

            if (hasOverlap)
            {
                throw new ConflictException(
                    "The selected venue already has an overlapping event.");
            }

            eventEntity.Name = dto.Name.Trim();
            eventEntity.Description =
                string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim();

            eventEntity.VenueId = dto.VenueId;
            eventEntity.EventCategoryId = dto.EventCategoryId;
            eventEntity.StartDateTime = dto.StartDateTime;
            eventEntity.EndDateTime = dto.EndDateTime;
            eventEntity.TicketPrice = dto.TicketPrice;
            eventEntity.ParkingFee = dto.ParkingFee;
            eventEntity.Capacity = dto.Capacity;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _eventRepository.UpdateAsync(eventEntity);

            eventEntity.Venue = venue;
            eventEntity.EventCategory = category;

            return MapToResponseDto(eventEntity);
        }

        public async Task DeleteAsync(int id)
        {
            var eventEntity =
                await _eventRepository.GetByIdAsync(id);

            if (eventEntity is null)
            {
                throw new NotFoundException(
                    "Event was not found.");
            }

            var hasBookings =
                await _eventRepository.HasBookingsAsync(id);

            if (hasBookings)
            {
                throw new ConflictException(
                    "This event cannot be deleted because it has existing bookings.");
            }

            await _eventRepository
                .DeleteWithResourcesAsync(eventEntity);
        }

        private static EventResponseDto MapToResponseDto(
            Event eventEntity)
        {
            return new EventResponseDto
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                VenueId = eventEntity.VenueId,
                VenueName =
                    eventEntity.Venue?.Name ?? string.Empty,
                EventCategoryId =
                    eventEntity.EventCategoryId,
                CategoryName =
                    eventEntity.EventCategory?.Name
                    ?? string.Empty,
                StartDateTime =
                    eventEntity.StartDateTime,
                EndDateTime =
                    eventEntity.EndDateTime,
                TicketPrice =
                    eventEntity.TicketPrice,
                ParkingFee =
                    eventEntity.ParkingFee,
                Capacity =
                    eventEntity.Capacity,
                CreatedAt =
                    eventEntity.CreatedAt,
                UpdatedAt =
                    eventEntity.UpdatedAt
            };
        }
    }
}
using Event_and_parking_reservation_system.DTOs.Venues;
using Event_and_parking_reservation_system.Exceptions;
using Event_and_parking_reservation_system.Interfaces.Repositories;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;

namespace Event_and_parking_reservation_system.Services
{
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _venueRepository;

        public VenueService(IVenueRepository venueRepository)
        {
            _venueRepository = venueRepository;
        }

        public async Task<IEnumerable<VenueResponseDto>> GetAllAsync()
        {
            var venues = await _venueRepository.GetAllAsync();

            return venues.Select(MapToResponseDto);
        }

        public async Task<VenueResponseDto> GetByIdAsync(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id);

            if (venue is null)
            {
                throw new NotFoundException("Venue was not found.");
            }

            return MapToResponseDto(venue);
        }

        public async Task<VenueResponseDto> CreateAsync(CreateVenueDto dto)
        {
            var venue = new Venue
            {
                Name = dto.Name.Trim(),
                Address = dto.Address.Trim(),
                TotalCapacity = dto.TotalCapacity,
                CreatedAt = DateTime.UtcNow
            };

            await _venueRepository.AddAsync(venue);

            return MapToResponseDto(venue);
        }

        public async Task<VenueResponseDto> UpdateAsync(
            int id,
            UpdateVenueDto dto)
        {
            var venue = await _venueRepository.GetByIdAsync(id);

            if (venue is null)
            {
                throw new NotFoundException("Venue was not found.");
            }

            venue.Name = dto.Name.Trim();
            venue.Address = dto.Address.Trim();
            venue.TotalCapacity = dto.TotalCapacity;
            venue.UpdatedAt = DateTime.UtcNow;

            await _venueRepository.UpdateAsync(venue);

            return MapToResponseDto(venue);
        }

        public async Task DeleteAsync(int id)
        {
            var venue = await _venueRepository.GetByIdAsync(id);

            if (venue is null)
            {
                throw new NotFoundException("Venue was not found.");
            }

            var hasUpcomingEvents =
                await _venueRepository.HasUpcomingEventsAsync(id);

            if (hasUpcomingEvents)
            {
                throw new ConflictException(
                    "Venue cannot be deleted because it has upcoming events.");
            }

            await _venueRepository.DeleteAsync(venue);
        }

        public async Task<IEnumerable<VenueAvailabilityResponseDto>>
            CheckAvailabilityAsync(VenueAvailabilityQueryDto query)
        {
            if (query.EndTime <= query.StartTime)
            {
                throw new ArgumentException(
                    "End time must be later than start time.");
            }

            var venues = await _venueRepository.GetAllAsync();

            if (query.VenueId.HasValue)
            {
                venues = venues.Where(
                    v => v.Id == query.VenueId.Value);
            }

            var results = new List<VenueAvailabilityResponseDto>();

            foreach (var venue in venues)
            {
                var isAvailable =
                    await _venueRepository.IsAvailableAsync(
                        venue.Id,
                        query.Date,
                        query.StartTime,
                        query.EndTime);

                results.Add(new VenueAvailabilityResponseDto
                {
                    VenueId = venue.Id,
                    VenueName = venue.Name,
                    IsAvailable = isAvailable
                });
            }

            return results;
        }

        private static VenueResponseDto MapToResponseDto(Venue venue)
        {
            return new VenueResponseDto
            {
                Id = venue.Id,
                Name = venue.Name,
                Address = venue.Address,
                TotalCapacity = venue.TotalCapacity,
                CreatedAt = venue.CreatedAt,
                UpdatedAt = venue.UpdatedAt
            };
        }
    }
}
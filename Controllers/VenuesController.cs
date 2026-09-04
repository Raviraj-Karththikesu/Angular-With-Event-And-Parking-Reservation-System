using Event_and_parking_reservation_system.DTOs.Venues;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VenueResponseDto>>> GetAll()
        {
            var venues = await _venueService.GetAllAsync();

            return Ok(venues);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VenueResponseDto>> GetById(int id)
        {
            var venue = await _venueService.GetByIdAsync(id);

            return Ok(venue);
        }

        [HttpPost]
        public async Task<ActionResult<VenueResponseDto>> Create(
            CreateVenueDto dto)
        {
            var venue = await _venueService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = venue.Id },
                venue);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<VenueResponseDto>> Update(
            int id,
            UpdateVenueDto dto)
        {
            var venue = await _venueService.UpdateAsync(id, dto);

            return Ok(venue);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _venueService.DeleteAsync(id);

            return NoContent();
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<VenueAvailabilityResponseDto>>>
            CheckAvailability(
                [FromQuery] VenueAvailabilityQueryDto query)
        {
            var result =
                await _venueService.CheckAvailabilityAsync(query);

            return Ok(result);
        }
    }
}
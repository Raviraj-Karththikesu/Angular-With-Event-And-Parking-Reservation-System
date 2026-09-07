using Event_and_parking_reservation_system.DTOs.Events;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAll(
            [FromQuery] EventFilterDto filter)
        {
            var events = await _eventService.GetAllAsync(filter);

            return Ok(events);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventResponseDto>> GetById(int id)
        {
            var eventItem = await _eventService.GetByIdAsync(id);

            return Ok(eventItem);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<EventResponseDto>> Create(
            CreateEventDto dto)
        {
            var eventItem = await _eventService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = eventItem.Id },
                eventItem);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<EventResponseDto>> Update(
            int id,
            UpdateEventDto dto)
        {
            var eventItem = await _eventService.UpdateAsync(id, dto);

            return Ok(eventItem);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Delete(int id)
        {
            await _eventService.DeleteAsync(id);

            return NoContent();
        }
    }
}
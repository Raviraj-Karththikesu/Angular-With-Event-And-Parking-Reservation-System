using Event_and_parking_reservation_system.DTOs.Seats;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:int}/seats")]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public SeatsController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        // GET: /api/events/{eventId}/seats
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<SeatResponseDto>>> GetSeatMap(
            int eventId)
        {
            var seats = await _seatService.GetSeatMapAsync(eventId);

            return Ok(seats);
        }

        // POST: /api/events/{eventId}/seats
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<SeatResponseDto>>> GenerateSeatMap(
            int eventId,
            [FromBody] GenerateSeatMapDto dto)
        {
            var seats = await _seatService.GenerateSeatMapAsync(
                eventId,
                dto);

            return CreatedAtAction(
                nameof(GetSeatMap),
                new { eventId },
                seats);
        }

        // PUT: /api/events/{eventId}/seats/{seatId}
        [HttpPut("{seatId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SeatResponseDto>> UpdateSeat(
            int eventId,
            int seatId,
            [FromBody] UpdateSeatDto dto)
        {
            var seat = await _seatService.UpdateSeatAsync(
                eventId,
                seatId,
                dto);

            return Ok(seat);
        }

        // DELETE: /api/events/{eventId}/seats/{seatId}
        [HttpDelete("{seatId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSeat(
            int eventId,
            int seatId)
        {
            await _seatService.DeleteSeatAsync(eventId, seatId);

            return NoContent();
        }
    }
}
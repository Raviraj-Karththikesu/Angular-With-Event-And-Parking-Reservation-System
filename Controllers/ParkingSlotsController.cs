using Event_and_parking_reservation_system.DTOs.Parking;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/events/{eventId:int}/parking-slots")]
    public class ParkingSlotsController : ControllerBase
    {
        private readonly IParkingSlotService _parkingSlotService;

        public ParkingSlotsController(IParkingSlotService parkingSlotService)
        {
            _parkingSlotService = parkingSlotService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ParkingSlotResponseDto>>> GetParkingLayout(
            int eventId)
        {
            var parkingSlots =
                await _parkingSlotService.GetParkingLayoutAsync(eventId);

            return Ok(parkingSlots);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ParkingSlotResponseDto>>> GenerateParkingLayout(
            int eventId,
            [FromBody] GenerateParkingLayoutDto dto)
        {
            var parkingSlots =
                await _parkingSlotService.GenerateParkingLayoutAsync(eventId, dto);

            return CreatedAtAction(
                nameof(GetParkingLayout),
                new { eventId },
                parkingSlots);
        }

        [HttpPut("{parkingSlotId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingSlotResponseDto>> UpdateParkingSlot(
            int eventId,
            int parkingSlotId,
            [FromBody] UpdateParkingSlotDto dto)
        {
            var parkingSlot =
                await _parkingSlotService.UpdateParkingSlotAsync(
                    eventId,
                    parkingSlotId,
                    dto);

            return Ok(parkingSlot);
        }

        [HttpDelete("{parkingSlotId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteParkingSlot(
            int eventId,
            int parkingSlotId)
        {
            await _parkingSlotService.DeleteParkingSlotAsync(
                eventId,
                parkingSlotId);

            return NoContent();
        }
    }
}
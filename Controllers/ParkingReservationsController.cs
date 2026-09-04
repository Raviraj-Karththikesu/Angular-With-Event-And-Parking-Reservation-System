using Event_and_parking_reservation_system.DTOs.ParkingReservations;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers;

[ApiController]
[Route("api/parking-reservations")]
[Authorize]
public class ParkingReservationsController : ControllerBase
{
    private readonly IParkingReservationService _parkingReservationService;

    public ParkingReservationsController(
        IParkingReservationService parkingReservationService)
    {
        _parkingReservationService = parkingReservationService;
    }

    [HttpPost]
    public async Task<ActionResult<ParkingReservationResponseDto>> ReserveParking(
        CreateParkingReservationDto dto)
    {
        var reservation =
            await _parkingReservationService.ReserveParkingAsync(dto);

        return CreatedAtAction(
            nameof(GetByBookingId),
            new { bookingId = reservation.BookingId },
            reservation);
    }

    [HttpGet("booking/{bookingId:int}")]
    public async Task<ActionResult<ParkingReservationResponseDto>> GetByBookingId(
        int bookingId)
    {
        var reservation =
            await _parkingReservationService.GetByBookingIdAsync(bookingId);

        if (reservation == null)
            return NotFound();

        return Ok(reservation);
    }

    [HttpDelete("booking/{bookingId:int}")]
    public async Task<IActionResult> ReleaseParking(int bookingId)
    {
        await _parkingReservationService.ReleaseParkingAsync(bookingId);

        return NoContent();
    }
}
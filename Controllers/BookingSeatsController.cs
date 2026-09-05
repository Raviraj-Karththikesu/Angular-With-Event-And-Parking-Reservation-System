using Event_and_parking_reservation_system.DTOs.BookingSeats;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers;

[ApiController]
[Route("api/bookings/{bookingId:int}/seats")]
[Authorize]
public class BookingSeatsController : ControllerBase
{
    private readonly IBookingSeatService _bookingSeatService;

    public BookingSeatsController(
        IBookingSeatService bookingSeatService)
    {
        _bookingSeatService = bookingSeatService;
    }

    [HttpPost]
    public async Task<ActionResult<List<BookingSeatResponseDto>>> AddSeats(
        int bookingId,
        [FromBody] AddBookingSeatsDto dto)
    {
        var result =
            await _bookingSeatService.AddSeatsToBookingAsync(
                bookingId,
                dto);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpGet]
    public async Task<ActionResult<List<BookingSeatResponseDto>>> GetSeats(
        int bookingId)
    {
        var result =
            await _bookingSeatService.GetBookingSeatsAsync(
                bookingId);

        return Ok(result);
    }
}
using Event_and_parking_reservation_system.DTOs.BookingSeats;
using Event_and_parking_reservation_system.Helpers;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers;

[ApiController]
[Route("api/bookings/{bookingId:int}/seats")]
[Authorize]
public class BookingSeatsController : ControllerBase
{
    private readonly IBookingSeatService
        _bookingSeatService;

    private readonly IBookingService
        _bookingService;

    public BookingSeatsController(
        IBookingSeatService bookingSeatService,
        IBookingService bookingService)
    {
        _bookingSeatService = bookingSeatService;
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<
        ActionResult<List<BookingSeatResponseDto>>>
        AddSeats(
            int bookingId,
            [FromBody] AddBookingSeatsDto dto)
    {
        await EnsureBookingAccessAsync(bookingId);

        List<BookingSeatResponseDto> result =
            await _bookingSeatService
                .AddSeatsToBookingAsync(
                    bookingId,
                    dto
                );

        return StatusCode(
            StatusCodes.Status201Created,
            result
        );
    }

    [HttpGet]
    public async Task<
        ActionResult<List<BookingSeatResponseDto>>>
        GetSeats(int bookingId)
    {
        await EnsureBookingAccessAsync(bookingId);

        List<BookingSeatResponseDto> result =
            await _bookingSeatService
                .GetBookingSeatsAsync(bookingId);

        return Ok(result);
    }

    private async Task EnsureBookingAccessAsync(
        int bookingId)
    {
        int requesterCustomerId =
            CurrentUserHelper.GetCustomerId(User);

        bool isAdmin =
            CurrentUserHelper.IsAdmin(User);

        await _bookingService.GetByIdAsync(
            requesterCustomerId,
            isAdmin,
            bookingId
        );
    }
}
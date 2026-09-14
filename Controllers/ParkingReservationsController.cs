using Event_and_parking_reservation_system
    .DTOs.ParkingReservations;
using Event_and_parking_reservation_system.Helpers;
using Event_and_parking_reservation_system
    .Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system
    .Controllers;

[ApiController]
[Route("api/parking-reservations")]
[Authorize]
public class ParkingReservationsController :
    ControllerBase
{
    private readonly IParkingReservationService
        _parkingReservationService;

    private readonly IBookingService
        _bookingService;

    public ParkingReservationsController(
        IParkingReservationService
            parkingReservationService,
        IBookingService bookingService)
    {
        _parkingReservationService =
            parkingReservationService;

        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<
        ActionResult<ParkingReservationResponseDto>>
        ReserveParking(
            [FromBody]
            CreateParkingReservationDto dto)
    {
        await EnsureBookingAccessAsync(
            dto.BookingId
        );

        ParkingReservationResponseDto reservation =
            await _parkingReservationService
                .ReserveParkingAsync(dto);

        return CreatedAtAction(
            nameof(GetByBookingId),
            new
            {
                bookingId =
                    reservation.BookingId
            },
            reservation
        );
    }

    [HttpGet("booking/{bookingId:int}")]
    public async Task<
        ActionResult<ParkingReservationResponseDto>>
        GetByBookingId(int bookingId)
    {
        await EnsureBookingAccessAsync(bookingId);

        ParkingReservationResponseDto? reservation =
            await _parkingReservationService
                .GetByBookingIdAsync(bookingId);

        if (reservation is null)
        {
            return NotFound(new
            {
                message =
                    "Parking reservation was not found."
            });
        }

        return Ok(reservation);
    }

    [HttpDelete("booking/{bookingId:int}")]
    public async Task<IActionResult>
        ReleaseParking(int bookingId)
    {
        await EnsureBookingAccessAsync(bookingId);

        await _parkingReservationService
            .ReleaseParkingAsync(bookingId);

        return NoContent();
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
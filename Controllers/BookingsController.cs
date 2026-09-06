using Event_and_parking_reservation_system.DTOs.Bookings;
using Event_and_parking_reservation_system.Helpers;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _service;

        public BookingsController(IBookingService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> Create(
            [FromBody] CreateBookingDto dto)
        {
            int customerId = CurrentUserHelper.GetCustomerId(User);
            BookingResponseDto result =
                await _service.CreateAsync(customerId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.BookingId },
                result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingResponseDto>> GetById(int id)
        {
            int customerId = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.GetByIdAsync(
                customerId,
                isAdmin,
                id));
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetCustomer(
            int customerId)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.GetCustomerBookingsAsync(
                requester,
                isAdmin,
                customerId));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetByEvent(
            [FromQuery] int eventId)
        {
            return Ok(await _service.GetEventBookingsAsync(eventId));
        }

      

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(int id)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            await _service.CancelAsync(requester, isAdmin, id);
            return NoContent();
        }
    }
}

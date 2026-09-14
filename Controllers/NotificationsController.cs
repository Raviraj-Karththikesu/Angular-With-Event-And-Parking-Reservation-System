using Event_and_parking_reservation_system.DTOs.Notifications;
using Event_and_parking_reservation_system.Helpers;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service)
        {
            _service = service;
        }

        // No public POST endpoint.
        // Notifications are created only by Booking/Payment/Event services.

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<List<NotificationResponseDto>>> GetCustomer(
            int customerId)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.GetCustomerNotificationsAsync(
                requester,
                isAdmin,
                customerId));
        }

        [HttpPut("{id:int}/read")]
        public async Task<ActionResult<NotificationResponseDto>> MarkRead(int id)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.MarkReadAsync(
                requester,
                isAdmin,
                id));
        }
    }
}

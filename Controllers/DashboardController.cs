using Event_and_parking_reservation_system.DTOs.Dashboards;
using Event_and_parking_reservation_system.Helpers;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminDashboardDto>> Admin()
        {
            return Ok(await _service.GetAdminSummaryAsync());
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<CustomerDashboardDto>> Customer(
            int customerId)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.GetCustomerSummaryAsync(
                requester,
                isAdmin,
                customerId));
        }
    }
}

using System.Security.Claims;
using Event_and_parking_reservation_system.DTOs.Customers;
using Event_and_parking_reservation_system.Interfaces.Services;
using Event_and_parking_reservation_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(
            ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(
            typeof(CustomerResponseDto),
            StatusCodes.Status201Created
        )]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest
        )]
        [ProducesResponseType(
            StatusCodes.Status409Conflict
        )]
        public async Task<ActionResult<CustomerResponseDto>>
            Register(
                [FromBody]
                RegisterCustomerDto registerCustomerDto)
        {
            CustomerResponseDto customer =
                await _customerService.RegisterAsync(
                    registerCustomerDto
                );

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.CustomerId },
                customer
            );
        }

        [Authorize(
            Roles = nameof(UserRole.Admin)
        )]
        [HttpGet]
        [ProducesResponseType(
            typeof(List<CustomerListItemDto>),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized
        )]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden
        )]
        public async Task<
            ActionResult<List<CustomerListItemDto>>>
            GetAll([FromQuery] string? search)
        {
            List<CustomerListItemDto> customers =
                await _customerService.SearchAsync(
                    search
                );

            return Ok(customers);
        }

        [Authorize(
            Roles = nameof(UserRole.Admin)
        )]
        [HttpGet("{id:int}")]
        [ProducesResponseType(
            typeof(CustomerResponseDto),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized
        )]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden
        )]
        [ProducesResponseType(
            StatusCodes.Status404NotFound
        )]
        public async Task<ActionResult<CustomerResponseDto>>
            GetById(int id)
        {
            CustomerResponseDto? customer =
                await _customerService.GetByIdAsync(
                    id
                );

            if (customer is null)
            {
                return NotFound(new
                {
                    message =
                        $"Customer with ID {id} was not found."
                });
            }

            return Ok(customer);
        }

        [Authorize(
            Roles = nameof(UserRole.Admin)
        )]
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(
            typeof(CustomerResponseDto),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest
        )]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized
        )]
        [ProducesResponseType(
            StatusCodes.Status403Forbidden
        )]
        [ProducesResponseType(
            StatusCodes.Status404NotFound
        )]
        public async Task<ActionResult<CustomerResponseDto>>
            UpdateStatus(
                int id,
                [FromBody]
                UpdateCustomerStatusDto updateStatusDto)
        {
            CustomerResponseDto customer =
                await _customerService
                    .UpdateStatusAsync(
                        id,
                        updateStatusDto
                    );

            return Ok(customer);
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(
            typeof(CustomerResponseDto),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized
        )]
        [ProducesResponseType(
            StatusCodes.Status404NotFound
        )]
        public async Task<ActionResult<CustomerResponseDto>>
            GetMyProfile()
        {
            string? customerIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!int.TryParse(
                customerIdValue,
                out int customerId))
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid authentication token."
                });
            }

            CustomerResponseDto? customer =
                await _customerService.GetByIdAsync(
                    customerId
                );

            if (customer is null)
            {
                return NotFound(new
                {
                    message =
                        "Customer was not found."
                });
            }

            return Ok(customer);
        }

        [Authorize]
        [HttpPut("me")]
        [ProducesResponseType(
            typeof(CustomerResponseDto),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(
            StatusCodes.Status400BadRequest
        )]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized
        )]
        [ProducesResponseType(
            StatusCodes.Status404NotFound
        )]
        [ProducesResponseType(
            StatusCodes.Status409Conflict
        )]
        public async Task<ActionResult<CustomerResponseDto>>
            UpdateMyProfile(
                [FromBody]
                UpdateCustomerProfileDto updateProfileDto)
        {
            string? customerIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!int.TryParse(
                customerIdValue,
                out int customerId))
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid authentication token."
                });
            }

            CustomerResponseDto customer =
                await _customerService
                    .UpdateProfileAsync(
                        customerId,
                        updateProfileDto
                    );

            return Ok(customer);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(
    typeof(CustomerResponseDto),
    StatusCodes.Status200OK
)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerResponseDto>>
    Deactivate(int id)
        {
            CustomerResponseDto customer =
                await _customerService.DeactivateAsync(id);

            return Ok(customer);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("{id:int}/reactivate")]
        [ProducesResponseType(
            typeof(CustomerResponseDto),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerResponseDto>>
            Reactivate(int id)
        {
            CustomerResponseDto customer =
                await _customerService.ReactivateAsync(id);

            return Ok(customer);
        }
    }
}
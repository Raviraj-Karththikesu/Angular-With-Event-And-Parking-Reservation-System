using Event_and_parking_reservation_system.DTOs.Customers;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost("register")]
        [ProducesResponseType(
            typeof(CustomerResponseDto),
            StatusCodes.Status201Created
        )]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerResponseDto>> Register(
            [FromBody] RegisterCustomerDto registerCustomerDto)
        {
            CustomerResponseDto customer =
                await _customerService.RegisterAsync(registerCustomerDto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.CustomerId },
                customer
            );
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(
            typeof(CustomerResponseDto),
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
        {
            CustomerResponseDto? customer =
                await _customerService.GetByIdAsync(id);

            if (customer is null)
            {
                return NotFound(new
                {
                    message = $"Customer with ID {id} was not found."
                });
            }

            return Ok(customer);
        }
    }
}
using Event_and_parking_reservation_system.DTOs.Payments;
using Event_and_parking_reservation_system.Helpers;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentsController(IPaymentService service)
        {
            _service = service;
        }

        [HttpGet("bookings/{bookingId:int}/payment")]
        public async Task<ActionResult<PaymentInfoDto>> GetPaymentInfo(
            int bookingId)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.GetPaymentInfoAsync(
                requester,
                isAdmin,
                bookingId));
        }

        [HttpPost("bookings/{bookingId:int}/payment")]
        public async Task<ActionResult<PaymentResponseDto>> Pay(int bookingId)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.PayAsync(
                requester,
                isAdmin,
                bookingId));
        }

        [HttpGet("payments/customer/{customerId:int}")]
        public async Task<ActionResult<List<PaymentResponseDto>>> GetHistory(
            int customerId)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.GetCustomerPaymentsAsync(
                requester,
                isAdmin,
                customerId));
        }

        [HttpGet("payments/{paymentId:int}/receipt")]
        public async Task<ActionResult<ReceiptResponseDto>> GetReceipt(
            int paymentId)
        {
            int requester = CurrentUserHelper.GetCustomerId(User);
            bool isAdmin = CurrentUserHelper.IsAdmin(User);

            return Ok(await _service.GetReceiptAsync(
                requester,
                isAdmin,
                paymentId));
        }
    }
}

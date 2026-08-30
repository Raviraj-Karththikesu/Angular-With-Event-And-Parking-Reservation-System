using System.Security.Claims;
using Event_and_parking_reservation_system.DTOs.Auth;
using Event_and_parking_reservation_system.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_and_parking_reservation_system.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailVerificationService
            _emailVerificationService;

        public AuthController(
            IAuthService authService,
            IEmailVerificationService emailVerificationService)
        {
            _authService = authService;
            _emailVerificationService = emailVerificationService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(
            typeof(AuthResponseDto),
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
        public async Task<ActionResult<AuthResponseDto>> Login(
            [FromBody] LoginRequestDto loginRequestDto)
        {
            AuthResponseDto response =
                await _authService.LoginAsync(
                    loginRequestDto
                );

            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(
            StatusCodes.Status200OK
        )]
        [ProducesResponseType(
            StatusCodes.Status401Unauthorized
        )]
        public IActionResult GetCurrentUser()
        {
            string? customerId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier
                )?.Value;

            string? fullName =
                User.FindFirst(
                    ClaimTypes.Name
                )?.Value;

            string? email =
                User.FindFirst(
                    ClaimTypes.Email
                )?.Value
                ?? User.FindFirst("email")?.Value;

            string? role =
                User.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            return Ok(new
            {
                customerId,
                fullName,
                email,
                role
            });
        }

        [AllowAnonymous]
        [HttpGet("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail(
    [FromQuery] string token)
        {
            await _emailVerificationService
                .VerifyEmailAsync(token);

            return Ok(new
            {
                message = "Email address verified successfully."
            });
        }

        [AllowAnonymous]
        [HttpPost("resend-verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerification(
    [FromBody]
    ResendVerificationRequestDto requestDto)
        {
            await _emailVerificationService
                .ResendVerificationEmailAsync(
                    requestDto.Email
                );

            return Ok(new
            {
                message =
                    "If an unverified account exists, " +
                    "a new verification email has been sent."
            });
        }
    }
}
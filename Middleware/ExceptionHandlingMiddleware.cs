using Event_and_parking_reservation_system.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Event_and_parking_reservation_system.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Application error: {Message}",
                    exception.Message
                );

                await WriteErrorResponseAsync(
                    context,
                    exception.StatusCode,
                    exception.Message
                );
            }
            catch (DbUpdateException exception)
            {
                _logger.LogError(
                    exception,
                    "A database update error occurred."
                );

                await WriteErrorResponseAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    "A database conflict occurred."
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An unexpected server error occurred."
                );

                await WriteErrorResponseAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected server error occurred."
                );
            }
        }

        private static async Task WriteErrorResponseAsync(
            HttpContext context,
            int statusCode,
            string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode,
                message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
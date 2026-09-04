using Event_and_parking_reservation_system.Data;
using Event_and_parking_reservation_system.Enums;
using Event_and_parking_reservation_system.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Event_and_parking_reservation_system.Middleware
{
    public class ActiveCustomerMiddleware
    {
        private readonly RequestDelegate _next;

        public ActiveCustomerMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            AppDbContext dbContext)
        {
            bool authenticated =
                context.User.Identity?.IsAuthenticated
                == true;

            if (!authenticated)
            {
                await _next(context);
                return;
            }

            string? customerIdValue =
                context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (!int.TryParse(
                customerIdValue,
                out int customerId))
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        statusCode =
                            StatusCodes
                                .Status401Unauthorized,

                        message =
                            "Invalid authentication token."
                    }
                );

                return;
            }

            var customer =
                await dbContext.Customers
                    .AsNoTracking()
                    .Where(item =>
                        item.Id == customerId
                    )
                    .Select(item => new
                    {
                        item.Status
                    })
                    .FirstOrDefaultAsync();

            if (customer is null)
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        statusCode =
                            StatusCodes
                                .Status401Unauthorized,

                        message =
                            "The customer account no longer exists."
                    }
                );

                return;
            }

            if (customer.Status ==
                CustomerStatus.Deactivated)
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;

                await context.Response.WriteAsJsonAsync(
                    new
                    {
                        statusCode =
                            StatusCodes
                                .Status403Forbidden,

                        message =
                            "Your account has been deactivated."
                    }
                );

                return;
            }

            await _next(context);
        }
    }
}
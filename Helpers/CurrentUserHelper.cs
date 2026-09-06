using System.Security.Claims;

namespace Event_and_parking_reservation_system.Helpers
{
    public static class CurrentUserHelper
    {
        public static int GetCustomerId(ClaimsPrincipal user)
        {
            string? value =
                user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                user.FindFirstValue("customerId") ??
                user.FindFirstValue("sub");

            if (!int.TryParse(value, out int customerId))
                throw new UnauthorizedAccessException(
                    "Customer id is missing from the JWT.");

            return customerId;
        }

        public static bool IsAdmin(ClaimsPrincipal user) =>
            user.IsInRole("Admin") ||
            string.Equals(
                user.FindFirstValue(ClaimTypes.Role),
                "Admin",
                StringComparison.OrdinalIgnoreCase);
    }
}


namespace Event_and_parking_reservation_system.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(
            string message,
            int statusCode = StatusCodes.Status400BadRequest)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
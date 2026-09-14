using Event_and_parking_reservation_system.Interfaces.Services;

namespace Event_and_parking_reservation_system.Workers
{
    public class BookingExpiryHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingExpiryHostedService> _logger;

        public BookingExpiryHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingExpiryHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();

                    IBookingService service =
                        scope.ServiceProvider.GetRequiredService<IBookingService>();

                    int expired = await service.ExpirePendingBookingsAsync();

                    if (expired > 0)
                        _logger.LogInformation(
                            "Expired {Count} pending booking(s).",
                            expired);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Booking expiry scan failed.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);
            }
        }
    }
}

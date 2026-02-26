using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Infrastructures;
using Microsoft.EntityFrameworkCore;

namespace Event_Management_System.API.Application.BackgroundServices
{
    public class ExpireReservedBookingService : BackgroundService
    {
        private readonly ILogger<ExpireReservedBookingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // check every 5 min

        public ExpireReservedBookingService(IServiceScopeFactory scopeFactory, ILogger<ExpireReservedBookingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("================= ExpireReservedBookingService is starting. ===================================");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireUnpaidBookingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while expiring unpaid bookings.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("================= ExpireReservedBookingService is stopping. ===================================");
        }

        private async Task ExpireUnpaidBookingsAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Use a fixed point in time for the entire batch
            var now = DateTimeOffset.UtcNow;

            // 1. Explicitly ensure we are tracking these entities
            var expiredUnpaidBookings = await dbContext.Bookings
                            .Where(b => b.BookingStatus == BookingStatus.PendingPayment && b.BookingReservationExpiresAt < now)
                            .ToListAsync(cancellationToken);

            if (expiredUnpaidBookings.Count == 0)
            {
                _logger.LogInformation("================> No expired bookings found at {Time}", now);
                return;
            }

            foreach (var booking in expiredUnpaidBookings)
            {
                booking.BookingStatus = BookingStatus.Expired;
                booking.ModifiedDate = now;

                // Use dbContext.Update to be safe if tracking is weird
                dbContext.Bookings.Update(booking);

                dbContext.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = booking.OrganizerId,
                    ObjectId = booking.Id,
                    ActionType = ActionType.Update,
                    Description = $"Reservation expired for booking {booking.Id}",
                    CreatedDate = now,
                    ModifiedDate = now
                });
            }

            // 2. Wrap the Save in a transaction or check the result
            var result = await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("=============> Successfully expired {Count} bookings and updated {Rows} rows.", expiredUnpaidBookings.Count, result);
        }
    }
}
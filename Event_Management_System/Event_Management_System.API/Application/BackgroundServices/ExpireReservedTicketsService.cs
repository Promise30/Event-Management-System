
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Infrastructures;
using Microsoft.EntityFrameworkCore;

namespace Event_Management_System.API.Application.BackgroundServices
{
    public class ExpireReservedTicketsService : BackgroundService
    {
        private readonly ILogger<ExpireReservedTicketsService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // check every 5 min

        public ExpireReservedTicketsService(IServiceScopeFactory scopeFactory, ILogger<ExpireReservedTicketsService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpireReservedTicketsService is starting.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireReservedTicketsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while expiring reserved tickets.");

                }
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
        private async Task ExpireReservedTicketsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTimeOffset.UtcNow;

            var expiredTickets = await dbContext.Tickets
                .Where(t => t.Status == TicketStatus.Reserved && t.ReservationExpiresAt < now)
                .ToListAsync();

            if (!expiredTickets.Any())
                return;
            foreach (var ticket in expiredTickets)
            {
                ticket.Status = TicketStatus.Cancelled;
                ticket.ModifiedDate = now;

                // Reduce SoldCount for ticket type
                var ticketType = await dbContext.TicketTypes.FirstOrDefaultAsync(tt => tt.Id == ticket.TicketTypeId);
                if (ticketType != null && ticketType.SoldCount > 0)
                {
                    ticketType.SoldCount -= 1;
                    ticketType.ModifiedDate = now;
                }
                dbContext.AuditLogs.Add(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = ticket.AttendeeId,
                    ObjectId = ticket.Id,
                    ActionType = ActionType.Update,
                    Description = $"Reservation expired for ticket {ticket.TicketNumber}",
                    CreatedDate = now,
                    ModifiedDate = now
                });
            }
            await dbContext.SaveChangesAsync();
            _logger.LogInformation($"Expired {expiredTickets.Count} reserved tickets.");
        }
    }
}

using Event_Management_System.API.Domain.Entities;

namespace Event_Management_System.API.Helpers
{
    public static class TicketHelper
    {
        public static string GenerateTicketNumber(Guid eventId, DateTime purchaseDate)
        {

            var dateString = purchaseDate.ToString("yyyyMMdd");
            var randomString = GenerateRandomString(6);
            return $"EVT-{eventId.ToString()[..8]}-{dateString}-{randomString}";
        }
        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        //public static bool CanPurchaseTickets(TicketType ticketType, int requestedQuantity)
        //{
        //    return ticketType.IsOnSale &&
        //           requestedQuantity <= ticketType.MaxTicketsPerPurchase &&
        //           requestedQuantity <= ticketType.AvailableCount;
        //}
    }
}


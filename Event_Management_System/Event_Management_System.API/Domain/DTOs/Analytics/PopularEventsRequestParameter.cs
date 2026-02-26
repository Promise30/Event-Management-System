namespace Event_Management_System.API.Domain.DTOs.Analytics
{
    public class PopularEventsRequestParameter
    {
        public int Top { get; set; } = 10;
        public string SortBy { get; set; } = "tickets";  
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

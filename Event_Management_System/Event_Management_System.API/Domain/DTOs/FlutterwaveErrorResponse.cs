namespace Event_Management_System.API.Domain.DTOs
{
    public class FlutterwaveErrorResponse
    {
        public string status { get; set; } = string.Empty;
        public FlutterwaveError error { get; set; } = new();
    }

    public class FlutterwaveError
    {
        public string type { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public object[] validation_errors { get; set; } = Array.Empty<object>();
    }
}

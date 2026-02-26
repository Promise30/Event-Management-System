using System.Text.Json.Serialization;

namespace Event_Management_System.API.Domain.DTOs.Customer
{
    /// <summary>
    /// Represents an error response from the Flutterwave API
    /// </summary>
    public class FlutterwaveErrorResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        public override string ToString()
        {
            return $"Flutterwave Error - Status: {Status}, Message: {Message}";
        }
    }
}

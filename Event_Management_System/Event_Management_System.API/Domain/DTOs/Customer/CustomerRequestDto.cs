using System.Text.Json.Serialization;

namespace Event_Management_System.API.Domain.DTOs.Customer
{
    /// <summary>
    /// Request DTO for creating a customer on Flutterwave
    /// </summary>
    public class CustomerRequestDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }
    }
}

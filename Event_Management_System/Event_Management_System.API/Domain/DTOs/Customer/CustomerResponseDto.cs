using System.Text.Json.Serialization;

namespace Event_Management_System.API.Domain.DTOs.Customer
{
    /// <summary>
    /// Response DTO for Flutterwave customer operations
    /// </summary>
    public class CustomerResponseDto
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public CustomerModel? Data { get; set; }
    }

    /// <summary>
    /// Customer data model returned by Flutterwave
    /// </summary>
    public class CustomerModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }
    }
}

using Event_Management_System.API.Domain.DTOs;
using Event_Management_System.API.Domain.DTOs.Customer;
using Event_Management_System.API.Domain.DTOs.Payment;
using Event_Management_System.API.Helpers;
using System.Net;

namespace Event_Management_System.API.Application
{
    public class TransactionService : ITransactionService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public TransactionService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
        // generate access token 
        public async Task<APIResponse<FlutterwaveTokenResponseDto>> CreateAccessToken()
        {
            var clientId = _configuration["Flutterwave:ClientId"];
            var clientSecret = _configuration["Flutterwave:ClientSecret"];
            var grantType = _configuration["Flutterwave:GrantType"];

            var url = "https://idp.flutterwave.com/realms/flutterwave/protocol/openid-connect/token";
            var client = _httpClientFactory.CreateClient();
            var requestBody = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", grantType }
            };
            var requestContent = new FormUrlEncodedContent(requestBody);
            var response = await client.PostAsync(url, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<FlutterwaveTokenResponseDto>(responseContent);
                return APIResponse<FlutterwaveTokenResponseDto>.Create(HttpStatusCode.OK, "Request Successful", apiResponse);
            }
            else
            {
                return APIResponse<FlutterwaveTokenResponseDto>.Create(HttpStatusCode.BadRequest, "Request Failed", null);
            }

        }
        // create customer
        public async Task<APIResponse<CustomerResponseDto>> CreateFlutterwaveCustomer(CustomerRequestDto customerRequestDto)
        {
            var client = _httpClientFactory.CreateClient("FlutterwaveClient");
            var request = new HttpRequestMessage(HttpMethod.Post, "customers");
            // Set headers
            request.Headers.Add("Authorization", $"Bearer {_configuration["Flutterwave:SecretKey"]}");
            request.Headers.Add("X-Trace-Id", Guid.NewGuid().ToString());
            request.Headers.Add("Content-Type", "application/json");
            // Set body
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(customerRequestDto);
            request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<CustomerResponseDto>(responseContent);
                return APIResponse<CustomerResponseDto>.Create(HttpStatusCode.OK, "Customer Created Successfully", apiResponse);
            }
            else
            {
                // use FlutterwaveErrorResponse to show more details of the error returned
                var errorResponse = System.Text.Json.JsonSerializer.Deserialize<FlutterwaveErrorResponse>(responseContent);
                return APIResponse<CustomerResponseDto>.Create(HttpStatusCode.BadRequest, errorResponse.ToString(), null);
            }
        }
        public async Task<APIResponse<CustomerResponseDto>> GetFlutterwaveCustomer(string customerId)
        {
            var client = _httpClientFactory.CreateClient("FlutterwaveClient");
            var request = new HttpRequestMessage(HttpMethod.Get, $"customers/{customerId}");
            // Set headers
            request.Headers.Add("Authorization", $"Bearer {_configuration["Flutterwave:SecretKey"]}");
            request.Headers.Add("X-Trace-Id", Guid.NewGuid().ToString());
            request.Headers.Add("Content-Type", "application/json");
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = System.Text.Json.JsonSerializer.Deserialize<CustomerResponseDto>(responseContent);
                return APIResponse<CustomerResponseDto>.Create(HttpStatusCode.OK, "Customer Retrieved Successfully", apiResponse);
            }
            else
            {
                // use FlutterwaveErrorResponse to show more details of the error returned
                var errorResponse = System.Text.Json.JsonSerializer.Deserialize<FlutterwaveErrorResponse>(responseContent);
                return APIResponse<CustomerResponseDto>.Create(HttpStatusCode.BadRequest, errorResponse.ToString(), null);
            }
        }
    }
}

using Event_Management_System.API.Domain.DTOs.Customer;
using Event_Management_System.API.Domain.DTOs.Payment;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<APIResponse<FlutterwaveTokenResponseDto>> CreateAccessToken();
        Task<APIResponse<CustomerResponseDto>> CreateFlutterwaveCustomer(CustomerRequestDto customerRequestDto);
        Task<APIResponse<CustomerResponseDto>> GetFlutterwaveCustomer(string customerId);
    }
}

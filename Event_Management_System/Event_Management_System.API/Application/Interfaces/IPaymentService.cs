using Event_Management_System.API.Domain.DTOs.Payment;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Payments
{
    public interface IPaymentService
    {
        /// <summary>
        /// Initialize a payment for a ticket purchase via Paystack
        /// </summary>
        Task<APIResponse<PaymentInitializationDto>> InitializePaymentAsync(InitiatePaymentDto dto);

        /// <summary>
        /// Verify a payment transaction via Paystack callback reference
        /// </summary>
        Task<APIResponse<PaymentVerificationDto>> VerifyPaymentAsync(string reference);

        /// <summary>
        /// Get payment by ID
        /// </summary>
        Task<APIResponse<PaymentInfoDto>> GetPaymentByIdAsync(Guid paymentId);

        /// <summary>
        /// Get payments for a user
        /// </summary>
        Task<APIResponse<IEnumerable<PaymentInfoDto>>> GetUserPaymentsAsync(Guid userId);

        /// <summary>
        /// Process Paystack webhook callback
        /// </summary>
        Task<APIResponse<object>> ProcessWebhookAsync(string payload, string paystackSignature);
    }
}

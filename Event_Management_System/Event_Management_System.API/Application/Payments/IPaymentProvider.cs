using Event_Management_System.API.Domain.DTOs.Payment;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Payments
{
    /// <summary>
    /// Abstraction for payment providers (Flutterwave, Paystack, etc.)
    /// </summary>
    public interface IPaymentProvider
    {
        /// <summary>
        /// Get the provider name (Flutterwave, Paystack)
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Initialize a payment transaction
        /// </summary>
        Task<APIResponse<PaymentInitializationDto>> InitializePaymentAsync(InitiatePaymentDto dto);

        /// <summary>
        /// Verify a payment transaction
        /// </summary>
        Task<APIResponse<PaymentVerificationDto>> VerifyPaymentAsync(string transactionReference);

        /// <summary>
        /// Process webhook callback from payment provider
        /// </summary>
        Task<APIResponse<PaymentVerificationDto>> ProcessWebhookAsync(string payload, Dictionary<string, string> headers);

        /// <summary>
        /// Refund a payment
        /// </summary>
        Task<APIResponse<RefundDto>> RefundPaymentAsync(string transactionReference, decimal amount, string reason);
    }
}

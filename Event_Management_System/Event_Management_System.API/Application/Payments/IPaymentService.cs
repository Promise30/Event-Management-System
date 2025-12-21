//using Event_Management_System.API.Domain.DTOs.Payment;
//using Event_Management_System.API.Helpers;

//namespace Event_Management_System.API.Application.Payments
//{
//    public interface IPaymentService
//    {
//        /// <summary>
//        /// Initialize a payment for booking or ticket
//        /// </summary>
//        Task<APIResponse<PaymentInitializationDto>> InitializePaymentAsync(
//            string provider, 
//            InitiatePaymentDto dto);

//        /// <summary>
//        /// Verify a payment transaction
//        /// </summary>
//        Task<APIResponse<PaymentVerificationDto>> VerifyPaymentAsync(
//            Guid paymentId);

//        /// <summary>
//        /// Process webhook from payment provider
//        /// </summary>
//        Task<APIResponse<object>> ProcessWebhookAsync(
//            string provider, 
//            string payload, 
//            Dictionary<string, string> headers);

//        /// <summary>
//        /// Refund a payment
//        /// </summary>
//        Task<APIResponse<RefundDto>> RefundPaymentAsync(
//            Guid paymentId, 
//            string reason);

//        /// <summary>
//        /// Get payment by ID
//        /// </summary>
//        Task<APIResponse<object>> GetPaymentByIdAsync(Guid paymentId);

//        /// <summary>
//        /// Get payments for a user
//        /// </summary>
//        Task<APIResponse<object>> GetUserPaymentsAsync(Guid userId);
//    }
//}

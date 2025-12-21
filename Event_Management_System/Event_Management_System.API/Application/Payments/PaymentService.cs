//using Event_Management_System.API.Domain.DTOs.Payment;
//using Event_Management_System.API.Domain.Entities;
//using Event_Management_System.API.Domain.Enums;
//using Event_Management_System.API.Helpers;
//using Event_Management_System.API.Infrastructures;
//using Microsoft.EntityFrameworkCore;
//using System.Net;
//using System.Text.Json;

//namespace Event_Management_System.API.Application.Payments
//{
//    public class PaymentService : IPaymentService
//    {
//        private readonly IEnumerable<IPaymentProvider> _paymentProviders;
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<PaymentService> _logger;

//        public PaymentService(
//            IEnumerable<IPaymentProvider> paymentProviders,
//            ApplicationDbContext context,
//            ILogger<PaymentService> logger)
//        {
//            _paymentProviders = paymentProviders;
//            _context = context;
//            _logger = logger;
//        }

//        public async Task<APIResponse<PaymentInitializationDto>> InitializePaymentAsync(
//            string providerName, 
//            InitiatePaymentDto dto)
//        {
//            try
//            {
//                // Get the specified provider
//                var provider = _paymentProviders.FirstOrDefault(p => 
//                    p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

//                if (provider == null)
//                {
//                    return APIResponse<PaymentInitializationDto>.Create(
//                        HttpStatusCode.BadRequest,
//                        $"Payment provider '{providerName}' not found",
//                        null,
//                        new Error { Message = "Invalid payment provider" });
//                }

//                // Initialize payment with provider
//                var result = await provider.InitializePaymentAsync(dto);

//                if (result.StatusCode == HttpStatusCode.OK)
//                {
//                    // Save payment record
//                    var payment = new Payment
//                    {
//                        Id = Guid.NewGuid(),
//                        UserId = dto.UserId,
//                        Provider = Enum.Parse<PaymentProvider>(providerName),
//                        TransactionReference = result.Data.TransactionReference,
//                        PaymentType = (int)dto.PaymentType,
//                        ReferenceId = dto.ReferenceId,
//                        Amount = dto.Amount,
//                        Currency = dto.Currency,
//                        Status = PaymentStatus.Pending,
//                        PaymentUrl = result.Data.PaymentUrl,
//                        Description = dto.Description,
//                        CustomerEmail = dto.Email,
//                        Metadata = JsonSerializer.Serialize(dto.Metadata),
//                        CreatedDate = DateTimeOffset.UtcNow
//                    };

//                    _context.Payments.Add(payment);
//                    await _context.SaveChangesAsync();

//                    _logger.LogInformation(
//                        "Payment initialized: {PaymentId} for user {UserId} via {Provider}",
//                        payment.Id, dto.UserId, providerName);
//                }

//                return result;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error initializing payment");
//                return APIResponse<PaymentInitializationDto>.Create(
//                    HttpStatusCode.InternalServerError,
//                    "An error occurred while initializing payment",
//                    null,
//                    new Error { Message = ex.Message });
//            }
//        }

//        public async Task<APIResponse<PaymentVerificationDto>> VerifyPaymentAsync(Guid paymentId)
//        {
//            try
//            {
//                var payment = await _context.Payments.FindAsync(paymentId);
//                if (payment == null)
//                {
//                    return APIResponse<PaymentVerificationDto>.Create(
//                        HttpStatusCode.NotFound,
//                        "Payment not found",
//                        null,
//                        new Error { Message = "Payment record does not exist" });
//                }

//                // Get provider
//                var provider = _paymentProviders.FirstOrDefault(p => 
//                    p.ProviderName.Equals(payment.Provider.ToString(), StringComparison.OrdinalIgnoreCase));

//                if (provider == null)
//                {
//                    return APIResponse<PaymentVerificationDto>.Create(
//                        HttpStatusCode.BadRequest,
//                        "Payment provider not found",
//                        null);
//                }

//                // Verify with provider
//                var result = await provider.VerifyPaymentAsync(payment.TransactionReference);

//                if (result.StatusCode == HttpStatusCode.OK && result.Data?.Status == "successful")
//                {
//                    // Update payment record
//                    payment.Status = PaymentStatus.Successful;
//                    payment.PaidAt = result.Data.PaidAt ?? DateTimeOffset.UtcNow;
//                    payment.ModifiedDate = DateTimeOffset.UtcNow;
//                    payment.ProviderReference = result.Data.TransactionReference;

//                    _context.Payments.Update(payment);

//                    // Update related booking or ticket
//                    await UpdateRelatedEntityAsync(payment);

//                    await _context.SaveChangesAsync();

//                    _logger.LogInformation("Payment verified successfully: {PaymentId}", paymentId);
//                }
//                else
//                {
//                    payment.Status = PaymentStatus.Failed;
//                    payment.ModifiedDate = DateTimeOffset.UtcNow;
//                    _context.Payments.Update(payment);
//                    await _context.SaveChangesAsync();
//                }

//                return result;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error verifying payment");
//                return APIResponse<PaymentVerificationDto>.Create(
//                    HttpStatusCode.InternalServerError,
//                    "An error occurred",
//                    null);
//            }
//        }

//        public async Task<APIResponse<object>> ProcessWebhookAsync(
//            string providerName, 
//            string payload, 
//            Dictionary<string, string> headers)
//        {
//            try
//            {
//                var provider = _paymentProviders.FirstOrDefault(p => 
//                    p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

//                if (provider == null)
//                {
//                    return APIResponse<object>.Create(
//                        HttpStatusCode.BadRequest,
//                        "Provider not found",
//                        null);
//                }

//                var result = await provider.ProcessWebhookAsync(payload, headers);

//                if (result.StatusCode == HttpStatusCode.OK && result.Data != null)
//                {
//                    // Find payment by transaction reference
//                    var payment = await _context.Payments
//                        .FirstOrDefaultAsync(p => p.TransactionReference == result.Data.TransactionReference);

//                    if (payment != null && payment.Status == PaymentStatus.Pending)
//                    {
//                        payment.Status = PaymentStatus.Successful;
//                        payment.PaidAt = result.Data.PaidAt ?? DateTimeOffset.UtcNow;
//                        payment.ModifiedDate = DateTimeOffset.UtcNow;
//                        _context.Payments.Update(payment);

//                        await UpdateRelatedEntityAsync(payment);
//                        await _context.SaveChangesAsync();

//                        _logger.LogInformation("Webhook processed for payment: {PaymentId}", payment.Id);
//                    }
//                }

//                return APIResponse<object>.Create(HttpStatusCode.OK, "Webhook processed", null);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error processing webhook");
//                return APIResponse<object>.Create(
//                    HttpStatusCode.InternalServerError,
//                    "An error occurred",
//                    null);
//            }
//        }

//        public async Task<APIResponse<RefundDto>> RefundPaymentAsync(Guid paymentId, string reason)
//        {
//            try
//            {
//                var payment = await _context.Payments.FindAsync(paymentId);
//                if (payment == null)
//                {
//                    return APIResponse<RefundDto>.Create(
//                        HttpStatusCode.NotFound,
//                        "Payment not found",
//                        null);
//                }

//                if (payment.Status != PaymentStatus.Successful)
//                {
//                    return APIResponse<RefundDto>.Create(
//                        HttpStatusCode.BadRequest,
//                        "Only successful payments can be refunded",
//                        null);
//                }

//                var provider = _paymentProviders.FirstOrDefault(p => 
//                    p.ProviderName.Equals(payment.Provider.ToString(), StringComparison.OrdinalIgnoreCase));

//                if (provider == null)
//                {
//                    return APIResponse<RefundDto>.Create(
//                        HttpStatusCode.BadRequest,
//                        "Provider not found",
//                        null);
//                }

//                var result = await provider.RefundPaymentAsync(
//                    payment.TransactionReference, 
//                    payment.Amount, 
//                    reason);

//                if (result.StatusCode == HttpStatusCode.OK)
//                {
//                    payment.Status = PaymentStatus.Refunded;
//                    payment.ModifiedDate = DateTimeOffset.UtcNow;
//                    _context.Payments.Update(payment);
//                    await _context.SaveChangesAsync();

//                    _logger.LogInformation("Payment refunded: {PaymentId}", paymentId);
//                }

//                return result;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error refunding payment");
//                return APIResponse<RefundDto>.Create(
//                    HttpStatusCode.InternalServerError,
//                    "An error occurred",
//                    null);
//            }
//        }

//        public async Task<APIResponse<object>> GetPaymentByIdAsync(Guid paymentId)
//        {
//            var payment = await _context.Payments
//                .Include(p => p.User)
//                .FirstOrDefaultAsync(p => p.Id == paymentId);

//            if (payment == null)
//            {
//                return APIResponse<object>.Create(
//                    HttpStatusCode.NotFound,
//                    "Payment not found",
//                    null);
//            }

//            return APIResponse<object>.Create(HttpStatusCode.OK, "Request successful", payment);
//        }

//        public async Task<APIResponse<object>> GetUserPaymentsAsync(Guid userId)
//        {
//            var payments = await _context.Payments
//                .Where(p => p.UserId == userId)
//                .OrderByDescending(p => p.CreatedDate)
//                .ToListAsync();

//            return APIResponse<object>.Create(HttpStatusCode.OK, "Request successful", payments);
//        }

//        private async Task UpdateRelatedEntityAsync(Payment payment)
//        {
//            try
//            {
//                if (payment.PaymentType == 1) // Booking
//                {
//                    var booking = await _context.Bookings.FindAsync(payment.ReferenceId);
//                    if (booking != null)
//                    {
//                        booking.BookingStatus = BookingStatus.Confirmed;
//                        booking.ModifiedDate = DateTimeOffset.UtcNow;
//                        _context.Bookings.Update(booking);
//                    }
//                }
//                else if (payment.PaymentType == 2) // Ticket
//                {
//                    var ticket = await _context.Tickets.FindAsync(payment.ReferenceId);
//                    if (ticket != null)
//                    {
//                        ticket.Status = TicketStatus.Confirmed;
//                        ticket.PaymentReference = payment.TransactionReference;
//                        ticket.ModifiedDate = DateTimeOffset.UtcNow;
//                        _context.Tickets.Update(ticket);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error updating related entity for payment {PaymentId}", payment.Id);
//            }
//        }
//    }
//}

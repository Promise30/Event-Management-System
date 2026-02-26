using Event_Management_System.API.Application.Payments;
using Event_Management_System.API.Domain.DTOs.Payment;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Domain.Enums;
using Event_Management_System.API.Helpers;
using Event_Management_System.API.Infrastructures;
using Microsoft.EntityFrameworkCore;
using PayStack.Net;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Event_Management_System.API.Application.Implementation
{
    public class PaystackPaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PaystackPaymentService> _logger;
        private readonly PayStackApi _payStackApi;
        private readonly IConfiguration _configuration;

        public PaystackPaymentService(
            ApplicationDbContext dbContext,
            ILogger<PaystackPaymentService> logger,
            PayStackApi payStackApi,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _payStackApi = payStackApi;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<APIResponse<PaymentInitializationDto>> InitializePaymentAsync(InitiatePaymentDto dto)
        {
            try
            {
                var transactionRequest = new TransactionInitializeRequest
                {
                    Email = dto.Email,
                    Reference = Guid.NewGuid().ToString(),
                    CallbackUrl = string.Concat(_configuration["PayStack:CallbackUrl"], "/api/payments/verify"),
                    AmountInKobo = (int)(dto.Amount * 100),
                    Currency = dto.Currency ?? "NGN",
                };

                var transactionResponse = _payStackApi.Transactions.Initialize(transactionRequest);

                if (!transactionResponse.Status)
                {
                    _logger.LogError("Paystack transaction initialization failed for user {UserId}", dto.UserId);
                    return APIResponse<PaymentInitializationDto>.Create(
                        HttpStatusCode.InternalServerError,
                        "An error occurred while trying to initiate payment",
                        null,
                        new Error { Message = "Payment initialization failed" });
                }

                // Create a Payment record
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    UserId = dto.UserId,
                    Provider = PaymentProvider.PayStack,
                    TransactionReference = transactionResponse.Data.Reference,
                    PaymentType = (int)dto.PaymentType,
                    ReferenceId = dto.ReferenceId,
                    Amount = dto.Amount,
                    Currency = dto.Currency ?? "NGN",
                    Status = PaymentStatus.Pending,
                    PaymentUrl = transactionResponse.Data.AuthorizationUrl,
                    Description = dto.Description,
                    ProviderReference = string.Empty,
                    CustomerEmail = dto.Email,
                    Metadata = JsonSerializer.Serialize(dto.Metadata),
                    CreatedDate = DateTimeOffset.UtcNow
                };

                _dbContext.Payments.Add(payment);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation(
                    "Payment initialized: {PaymentId} with reference {Reference} for user {UserId}",
                    payment.Id, transactionResponse.Data.Reference, dto.UserId);

                var result = new PaymentInitializationDto
                {
                    TransactionReference = transactionResponse.Data.Reference,
                    PaymentUrl = transactionResponse.Data.AuthorizationUrl,
                    Amount = dto.Amount,
                    Currency = dto.Currency ?? "NGN",
                    Provider = "PayStack"
                };

                return APIResponse<PaymentInitializationDto>.Create(
                    HttpStatusCode.OK,
                    "Payment initialized successfully",
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing payment for user {UserId}", dto.UserId);
                return APIResponse<PaymentInitializationDto>.Create(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while initializing payment",
                    null,
                    new Error { Message = ex.Message });
            }
        }

        /// <inheritdoc/>
        public async Task<APIResponse<PaymentVerificationDto>> VerifyPaymentAsync(string reference)
        {
            try
            {
                var payment = await _dbContext.Payments
                    .FirstOrDefaultAsync(p => p.TransactionReference == reference);

                if (payment == null)
                {
                    return APIResponse<PaymentVerificationDto>.Create(
                        HttpStatusCode.NotFound,
                        "Payment not found",
                        null,
                        new Error { Message = "No payment record found for the provided reference" });
                }

                if (payment.Status == PaymentStatus.Successful)
                {
                    return APIResponse<PaymentVerificationDto>.Create(
                        HttpStatusCode.OK,
                        "Payment already verified",
                        new PaymentVerificationDto
                        {
                            TransactionReference = payment.TransactionReference,
                            Status = "successful",
                            Amount = payment.Amount,
                            Currency = payment.Currency,
                            CustomerEmail = payment.CustomerEmail,
                            PaidAt = payment.PaidAt
                        });
                }

                // Verify with Paystack
                var verificationResponse = _payStackApi.Transactions.Verify(reference);

                if (!verificationResponse.Status || verificationResponse.Data.Status != "success")
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.ModifiedDate = DateTimeOffset.UtcNow;
                    await _dbContext.SaveChangesAsync();

                    _logger.LogWarning("Payment verification failed for reference {Reference}", reference);

                    return APIResponse<PaymentVerificationDto>.Create(
                        HttpStatusCode.BadRequest,
                        "Payment verification failed",
                        null,
                        new Error { Message = "Payment could not be verified" });
                }

                // Update payment record
                payment.Status = PaymentStatus.Successful;
                payment.PaidAt = DateTimeOffset.UtcNow;
                payment.ModifiedDate = DateTimeOffset.UtcNow;
                payment.ProviderReference = verificationResponse.Data.Reference;

                // Update related entity (ticket or booking)
                await UpdateRelatedEntityAsync(payment);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Payment verified successfully for reference {Reference}", reference);

                var result = new PaymentVerificationDto
                {
                    TransactionReference = payment.TransactionReference,
                    Status = "successful",
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    CustomerEmail = payment.CustomerEmail,
                    PaidAt = payment.PaidAt
                };

                return APIResponse<PaymentVerificationDto>.Create(
                    HttpStatusCode.OK,
                    "Payment verified successfully",
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment for reference {Reference}", reference);
                return APIResponse<PaymentVerificationDto>.Create(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while verifying payment",
                    null,
                    new Error { Message = ex.Message });
            }
        }

        /// <inheritdoc/>
        public async Task<APIResponse<PaymentInfoDto>> GetPaymentByIdAsync(Guid paymentId)
        {
            try
            {
                var payment = await _dbContext.Payments
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    return APIResponse<PaymentInfoDto>.Create(
                        HttpStatusCode.NotFound,
                        "Payment not found",
                        null,
                        new Error { Message = "Payment record does not exist" });
                }

                var response = new PaymentInfoDto
                {
                    PaymentId = paymentId,
                    Reference = payment.TransactionReference,
                    Description = payment.Description,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Email = payment.CustomerEmail,
                    Status = payment.Status.GetEnumDescription(),
                    PaymentProvider = payment.Provider.GetEnumDescription(),
                    PaidAt = payment.PaidAt,
                    CreatedDate = payment.CreatedDate,
                };
                return APIResponse<PaymentInfoDto>.Create(HttpStatusCode.OK, "Request successful", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", paymentId);
                return APIResponse<PaymentInfoDto>.Create(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while retrieving payment",
                    null,
                    new Error { Message = ex.Message });
            }
        }

        /// <inheritdoc/>
        public async Task<APIResponse<IEnumerable<PaymentInfoDto>>> GetUserPaymentsAsync(Guid userId)
        {
            try
            {
                var payments = await _dbContext.Payments
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(p => new PaymentInfoDto
                    {
                        PaymentId = p.Id,
                        Reference = p.TransactionReference,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Email = p.CustomerEmail,
                        Status = p.Status.GetEnumDescription(),
                        PaymentProvider = p.Provider.GetEnumDescription(),
                        PaidAt = p.PaidAt,
                        CreatedDate = p.CreatedDate,
                        Description = p.Description
                    })
                    .ToListAsync();

                return APIResponse<IEnumerable<PaymentInfoDto>>.Create(HttpStatusCode.OK, "Request successful", payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for user {UserId}", userId);
                return APIResponse<IEnumerable<PaymentInfoDto>>.Create(
                    HttpStatusCode.InternalServerError,
                    "An error occurred while retrieving payments",
                    null,
                    new Error { Message = ex.Message });
            }
        }

        /// <inheritdoc/>
        public async Task<APIResponse<object>> ProcessWebhookAsync(string payload, string paystackSignature)
        {
            try
            {
                // Validate webhook signature
                var secretKey = _configuration["PayStack:SecretKey"];
                var computedSignature = ComputeHmacSha512(secretKey!, payload);

                if (!string.Equals(computedSignature, paystackSignature, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid Paystack webhook signature received");
                    return APIResponse<object>.Create(HttpStatusCode.Unauthorized, "Invalid signature", null);
                }

                var webhookEvent = JsonSerializer.Deserialize<JsonElement>(payload);
                var eventType = webhookEvent.GetProperty("event").GetString();

                if (eventType == "charge.success")
                {
                    var data = webhookEvent.GetProperty("data");
                    var reference = data.GetProperty("reference").GetString();

                    if (!string.IsNullOrEmpty(reference))
                    {
                        var payment = await _dbContext.Payments
                            .FirstOrDefaultAsync(p => p.TransactionReference == reference);

                        if (payment != null && payment.Status == PaymentStatus.Pending)
                        {
                            payment.Status = PaymentStatus.Successful;
                            payment.PaidAt = DateTimeOffset.UtcNow;
                            payment.ModifiedDate = DateTimeOffset.UtcNow;

                            await UpdateRelatedEntityAsync(payment);
                            await _dbContext.SaveChangesAsync();

                            _logger.LogInformation("Webhook processed: payment {PaymentId} marked as successful", payment.Id);
                        }
                    }
                }

                return APIResponse<object>.Create(HttpStatusCode.OK, "Webhook processed", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Paystack webhook");
                return APIResponse<object>.Create(
                    HttpStatusCode.InternalServerError,
                    "An error occurred processing webhook",
                    null,
                    new Error { Message = ex.Message });
            }
        }

        #region Private helpers

        private async Task UpdateRelatedEntityAsync(Payment payment)
        {
            try
            {
                if (payment.PaymentType == (int)PaymentType.Booking)
                {
                    var booking = await _dbContext.Bookings.FindAsync(payment.ReferenceId);
                    if (booking != null)
                    {
                        booking.BookingStatus = BookingStatus.PendingApproval;
                        booking.PaymentReference = payment.TransactionReference;
                        booking.PaymentCompletedAt = payment.PaidAt;
                        booking.ModifiedDate = DateTimeOffset.UtcNow;
                        _dbContext.Bookings.Update(booking);
                    }
                }
                else if (payment.PaymentType == (int)PaymentType.Ticket)
                {
                    var ticket = await _dbContext.Tickets.FindAsync(payment.ReferenceId);
                    if (ticket != null)
                    {
                        ticket.Status = TicketStatus.Active;
                        ticket.PaymentReference = payment.TransactionReference;
                        ticket.PaymentCompletedAt = payment.PaidAt;
                        ticket.ModifiedDate = DateTimeOffset.UtcNow;
                        _dbContext.Tickets.Update(ticket);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating related entity for payment {PaymentId}", payment.Id);
            }
        }

        private static string ComputeHmacSha512(string secretKey, string payload)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA512(keyBytes);
            var hashBytes = hmac.ComputeHash(payloadBytes);
            return Convert.ToHexStringLower(hashBytes);
        }

        #endregion
    }
}

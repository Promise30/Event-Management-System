using Event_Management_System.API.Application.Payments;
using Event_Management_System.API.Domain.DTOs.Payment;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Manages Paystack payment operations including initialization, verification, and payment history
    /// </summary>
    [Authorize]
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(
            IHttpContextAccessor contextAccessor,
            IConfiguration configuration,
            IPaymentService paymentService) : base(contextAccessor, configuration)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Initialize a payment transaction via Paystack for a booking or ticket purchase
        /// </summary>
        /// <param name="dto">Payment initialization details including amount, email, and payment type</param>
        /// <returns>The Paystack authorization URL and transaction reference</returns>
        /// <response code="200">Payment initialized successfully with Paystack authorization URL</response>
        /// <response code="400">Invalid payment data</response>
        /// <response code="500">An internal server error occurred or Paystack initialization failed</response>
        [HttpPost("initialize")]
        [ProducesResponseType(typeof(APIResponse<PaymentInitializationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitializePayment([FromBody] InitiatePaymentDto dto)
        {
            dto.UserId = GetUserId();
            var result = await _paymentService.InitializePaymentAsync(dto);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Verify a Paystack payment transaction using the transaction reference.
        /// This is the callback URL that Paystack redirects to after payment completion.
        /// On successful verification, the related ticket or booking is automatically activated/confirmed.
        /// </summary>
        /// <param name="reference">The Paystack transaction reference returned during initialization</param>
        /// <returns>Payment verification details including status and amount</returns>
        /// <response code="200">Payment verified successfully</response>
        /// <response code="400">Payment verification failed or invalid reference</response>
        /// <response code="404">Payment record not found for the provided reference</response>
        /// <response code="500">An internal server error occurred</response>
        [AllowAnonymous]
        [HttpGet("verify")]
        [ProducesResponseType(typeof(APIResponse<PaymentVerificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyPayment([FromQuery] string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return BadRequest("Transaction reference is required");
            }

            var result = await _paymentService.VerifyPaymentAsync(reference);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Retrieve a specific payment record by its unique identifier
        /// </summary>
        /// <param name="paymentId">The unique identifier of the payment</param>
        /// <returns>The payment details</returns>
        /// <response code="200">Payment retrieved successfully</response>
        /// <response code="404">Payment not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(APIResponse<PaymentInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPayment(Guid paymentId)
        {
            var result = await _paymentService.GetPaymentByIdAsync(paymentId);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Retrieve the payment history for the currently authenticated user
        /// </summary>
        /// <returns>A list of all payments made by the user, ordered by most recent first</returns>
        /// <response code="200">Payment history retrieved successfully</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet("user/history")]
        [ProducesResponseType(typeof(APIResponse<List<PaymentInfoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserPayments()
        {
            var userId = GetUserId();
            var result = await _paymentService.GetUserPaymentsAsync(userId);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}

using Event_Management_System.API.Application.Payments;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Handles payment provider webhook callbacks
    /// </summary>
    [Route("api/webhooks")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IPaymentService paymentService,
            ILogger<WebhookController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Paystack webhook endpoint. Paystack sends event notifications here.
        /// </summary>
        [HttpPost("paystack")]
        public async Task<IActionResult> PaystackWebhook()
        {
            try
            {
                _logger.LogInformation("------> Received webhook from paystack at {date}",  DateTimeOffset.UtcNow);
                using var reader = new StreamReader(Request.Body);
                var payload = await reader.ReadToEndAsync();

                // Paystack sends the HMAC signature in the x-paystack-signature header
                var paystackSignature = Request.Headers["x-paystack-signature"].FirstOrDefault() ?? string.Empty;

                _logger.LogInformation("Received Paystack webhook: {webhook}", payload);

                var result = await _paymentService.ProcessWebhookAsync(payload, paystackSignature);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Paystack webhook");
                return StatusCode(500);
            }
        }
    }
}

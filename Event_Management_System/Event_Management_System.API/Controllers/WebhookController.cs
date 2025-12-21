//using Event_Management_System.API.Application.Payments;
//using Microsoft.AspNetCore.Mvc;

//namespace Event_Management_System.API.Controllers
//{
//    [Route("api/webhooks")]
//    [ApiController]
//    public class WebhookController : ControllerBase
//    {
//        private readonly IPaymentService _paymentService;
//        private readonly ILogger<WebhookController> _logger;

//        public WebhookController(
//            IPaymentService paymentService,
//            ILogger<WebhookController> logger)
//        {
//            _paymentService = paymentService;
//            _logger = logger;
//        }

//        /// <summary>
//        /// Flutterwave webhook endpoint
//        /// </summary>
//        [HttpPost("flutterwave")]
//        public async Task<IActionResult> FlutterwaveWebhook()
//        {
//            try
//            {
//                using var reader = new StreamReader(Request.Body);
//                var payload = await reader.ReadToEndAsync();

//                var headers = Request.Headers
//                    .ToDictionary(h => h.Key, h => h.Value.ToString());

//                _logger.LogInformation("Received Flutterwave webhook");

//                var result = await _paymentService.ProcessWebhookAsync("Flutterwave", payload, headers);
                
//                return Ok(new { status = "success" });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error processing Flutterwave webhook");
//                return StatusCode(500);
//            }
//        }

//        /// <summary>
//        /// Paystack webhook endpoint
//        /// </summary>
//        [HttpPost("paystack")]
//        public async Task<IActionResult> PaystackWebhook()
//        {
//            try
//            {
//                using var reader = new StreamReader(Request.Body);
//                var payload = await reader.ReadToEndAsync();

//                var headers = Request.Headers
//                    .ToDictionary(h => h.Key, h => h.Value.ToString());

//                _logger.LogInformation("Received Paystack webhook");

//                var result = await _paymentService.ProcessWebhookAsync("Paystack", payload, headers);
                
//                return Ok();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error processing Paystack webhook");
//                return StatusCode(500);
//            }
//        }
//    }
//}

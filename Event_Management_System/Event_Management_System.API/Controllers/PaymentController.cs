//using Event_Management_System.API.Application.Payments;
//using Event_Management_System.API.Domain.DTOs.Payment;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Net;

//namespace Event_Management_System.API.Controllers
//{
//    [Authorize]
//    [Route("api/payments")]
//    [ApiController]
//    public class PaymentController : BaseController
//    {
//        private readonly IPaymentService _paymentService;

//        public PaymentController(
//            IHttpContextAccessor contextAccessor,
//            IConfiguration configuration,
//            IPaymentService paymentService) : base(contextAccessor, configuration)
//        {
//            _paymentService = paymentService;
//        }

//        /// <summary>
//        /// Initialize a payment for booking or ticket
//        /// </summary>
//        /// <param name="provider">Payment provider (Flutterwave or Paystack)</param>
//        /// <param name="dto">Payment initialization details</param>
//        [HttpPost("initialize")]
//        public async Task<IActionResult> InitializePayment(
//            [FromQuery] string provider,
//            [FromBody] InitiatePaymentDto dto)
//        {
//            dto.UserId = GetUserId();
//            var result = await _paymentService.InitializePaymentAsync(provider, dto);
//            return result.StatusCode == HttpStatusCode.OK 
//                ? Ok(result) 
//                : StatusCode((int)result.StatusCode, result);
//        }

//        /// <summary>
//        /// Verify a payment transaction
//        /// </summary>
//        [HttpGet("verify/{paymentId}")]
//        public async Task<IActionResult> VerifyPayment(Guid paymentId)
//        {
//            var result = await _paymentService.VerifyPaymentAsync(paymentId);
//            return result.StatusCode == HttpStatusCode.OK 
//                ? Ok(result) 
//                : StatusCode((int)result.StatusCode, result);
//        }

//        /// <summary>
//        /// Get payment by ID
//        /// </summary>
//        [HttpGet("{paymentId}")]
//        public async Task<IActionResult> GetPayment(Guid paymentId)
//        {
//            var result = await _paymentService.GetPaymentByIdAsync(paymentId);
//            return result.StatusCode == HttpStatusCode.OK 
//                ? Ok(result) 
//                : StatusCode((int)result.StatusCode, result);
//        }

//        /// <summary>
//        /// Get user's payment history
//        /// </summary>
//        [HttpGet("user/history")]
//        public async Task<IActionResult> GetUserPayments()
//        {
//            var userId = GetUserId();
//            var result = await _paymentService.GetUserPaymentsAsync(userId);
//            return result.StatusCode == HttpStatusCode.OK 
//                ? Ok(result) 
//                : StatusCode((int)result.StatusCode, result);
//        }

//        /// <summary>
//        /// Refund a payment
//        /// </summary>
//        [HttpPost("{paymentId}/refund")]
//        [Authorize(Roles = "Administrator")]
//        public async Task<IActionResult> RefundPayment(
//            Guid paymentId,
//            [FromBody] RefundRequestDto dto)
//        {
//            var result = await _paymentService.RefundPaymentAsync(paymentId, dto.Reason);
//            return result.StatusCode == HttpStatusCode.OK 
//                ? Ok(result) 
//                : StatusCode((int)result.StatusCode, result);
//        }
//    }

//    public class RefundRequestDto
//    {
//        public string Reason { get; set; }
//    }
//}

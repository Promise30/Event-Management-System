using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Event_Management_System.API.Controllers
{
    public class BaseController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        public BaseController(IHttpContextAccessor contextAccessor, IConfiguration configuration)
        {
            _contextAccessor = contextAccessor;
            _configuration = configuration;
        }
        [NonAction]
        public Guid GetUserId()
        {
            ClaimsPrincipal user = _contextAccessor.HttpContext?.User;
            if (user == null) 
                return Guid.Empty;
            Claim userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            string userId = userIdClaim?.Value ?? throw new ArgumentNullException(nameof(userIdClaim));

            return Guid.Parse(userId);
        }

        [NonAction]
        public string GetUserRole()
        {
            ClaimsPrincipal user = _contextAccessor.HttpContext?.User;

            if (user == null)
                throw new InvalidOperationException("User is not authenticated.");

            Claim roleClaim = user.FindFirst(ClaimTypes.Role);

            return roleClaim?.Value ?? throw new InvalidOperationException("User role not found.");
        }
        [NonAction]
        public string GetIPAddress()
        {
            var ipAddress = _contextAccessor.HttpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault()
                                ?? _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            return ipAddress ?? "";

        }
    }
}

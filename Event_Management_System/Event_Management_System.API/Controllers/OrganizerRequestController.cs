using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Event_Management_System.API.Controllers
{
    /// <summary>
    /// Manages organizer elevation requests. Users can request to become Organizers; Admins approve or reject.
    /// </summary>
    [Authorize]
    [Route("api/organizer-requests")]
    [ApiController]
    public class OrganizerRequestController : BaseController
    {
        private readonly IOrganizerRequestService _organizerRequestService;

        public OrganizerRequestController(
            IHttpContextAccessor contextAccessor,
            IConfiguration configuration,
            IOrganizerRequestService organizerRequestService) : base(contextAccessor, configuration)
        {
            _organizerRequestService = organizerRequestService;
        }

        /// <summary>
        /// Submit a request to become an Organizer. Only users with the 'User' role can submit requests.
        /// </summary>
        /// <param name="dto">Optional reason/motivation for the request</param>
        /// <returns>The created organizer request</returns>
        /// <response code="201">Request submitted successfully</response>
        /// <response code="400">User already has a pending request or is not eligible</response>
        /// <response code="404">User not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(typeof(APIResponse<OrganizerRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmitRequest([FromBody] CreateOrganizerRequestDto dto)
        {
            var userId = GetUserId();
            var result = await _organizerRequestService.SubmitRequestAsync(userId, dto);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Retrieve all pending organizer requests. Admin only.
        /// </summary>
        /// <returns>A list of pending organizer requests</returns>
        /// <response code="200">Pending requests retrieved successfully</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(APIResponse<IEnumerable<OrganizerRequestDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPendingRequests()
        {
            var result = await _organizerRequestService.GetPendingRequestsAsync();
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Approve an organizer request. Assigns the Organizer role to the user and removes the User role. Admin only.
        /// </summary>
        /// <param name="id">The unique identifier of the organizer request</param>
        /// <param name="dto">Optional admin note</param>
        /// <returns>The updated organizer request</returns>
        /// <response code="200">Request approved successfully</response>
        /// <response code="400">Request is not in a pending state</response>
        /// <response code="404">Request not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(APIResponse<OrganizerRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ReviewOrganizerRequestDto dto)
        {
            var adminUserId = GetUserId();
            var result = await _organizerRequestService.ApproveRequestAsync(adminUserId, id, dto);
            return StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Reject an organizer request. Admin only.
        /// </summary>
        /// <param name="id">The unique identifier of the organizer request</param>
        /// <param name="dto">Optional admin note explaining the rejection</param>
        /// <returns>The updated organizer request</returns>
        /// <response code="200">Request rejected</response>
        /// <response code="400">Request is not in a pending state</response>
        /// <response code="404">Request not found</response>
        /// <response code="500">An internal server error occurred</response>
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(APIResponse<OrganizerRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectRequest(Guid id, [FromBody] ReviewOrganizerRequestDto dto)
        {
            var adminUserId = GetUserId();
            var result = await _organizerRequestService.RejectRequestAsync(adminUserId, id, dto);
            return StatusCode((int)result.StatusCode, result);
        }
    }
}

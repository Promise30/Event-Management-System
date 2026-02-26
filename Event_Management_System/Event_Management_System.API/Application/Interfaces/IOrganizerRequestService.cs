using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Helpers;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface IOrganizerRequestService
    {
        /// <summary>
        /// Submit a request to become an Organizer (User role only)
        /// </summary>
        Task<APIResponse<OrganizerRequestDto>> SubmitRequestAsync(Guid userId, CreateOrganizerRequestDto dto);

        /// <summary>
        /// Get all pending organizer requests (Admin only)
        /// </summary>
        Task<APIResponse<IEnumerable<OrganizerRequestDto>>> GetPendingRequestsAsync();

        /// <summary>
        /// Approve an organizer request (Admin only)
        /// </summary>
        Task<APIResponse<OrganizerRequestDto>> ApproveRequestAsync(Guid adminUserId, Guid requestId, ReviewOrganizerRequestDto dto);

        /// <summary>
        /// Reject an organizer request (Admin only)
        /// </summary>
        Task<APIResponse<OrganizerRequestDto>> RejectRequestAsync(Guid adminUserId, Guid requestId, ReviewOrganizerRequestDto dto);
    }
}

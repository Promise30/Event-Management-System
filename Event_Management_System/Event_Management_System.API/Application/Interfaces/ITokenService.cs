using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Domain.Entities;
using System.Security.Claims;

namespace Event_Management_System.API.Application.Interfaces
{
    public interface ITokenService
    {
        Task<TokenDto> CreateToken(bool populateExp, ApplicationUser user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

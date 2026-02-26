using Event_Management_System.API.Application.Interfaces;
using Event_Management_System.API.Domain.DTOs.User;
using Event_Management_System.API.Domain.Entities;
using Event_Management_System.API.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Event_Management_System.API.Application.Implementation
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _configuration = configuration;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<TokenDto> CreateToken(bool populateExp, ApplicationUser user)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims(user);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            if (populateExp)
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
        }
        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private async Task<List<Claim>> GetClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
             new Claim(ClaimTypes.Name, user.UserName),
             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
             new Claim(ClaimTypes.Email, user.Email),
             };
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken
            (
            issuer: _jwtSettings.ValidIssuer,
            audience: _jwtSettings.ValidAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToUInt32(_jwtSettings.Expires)),
            signingCredentials: signingCredentials
            );
            return tokenOptions;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateLifetime = false,
                ValidIssuer = _jwtSettings.ValidIssuer,
                ValidAudience = _jwtSettings.ValidAudience,
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
            StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }
    }
}

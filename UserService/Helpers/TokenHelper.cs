using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.Entity;

namespace UserService.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateToken(User user, JwtSettings settings)
        {
            var claims = new List<Claim>
                {
                    new Claim("userId", user.Id.ToString()),
                    new Claim("email", user.Email),
                    new Claim("role", user.Role.ToString())
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;

            var token = new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(settings.AccessTokenExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

}

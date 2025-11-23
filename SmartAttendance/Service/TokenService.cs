using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartAttendance.Models;
using SmartAttendance.Interfaces;

namespace SmartAttendance.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            // Read the same keys as appsettings.json: Jwt:Key, Jwt:Issuer, Jwt:Audience
            var signingKey = _config["Jwt:Key"] ?? _config["JWT:SigningKey"] ?? string.Empty;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        }

        // 🔹 Generate JWT Token for logged-in user
        public string CreateToken(AppUser user, IEnumerable<string>? roles = null)
        {
            // ✅ Claims: These are pieces of info stored inside the token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),           // User ID
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""), // User Email
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? "") // Username
            };

            // Add role claims from passed-in roles (preferred) or fall back to AppUser.Role
            if (roles != null)
            {
                foreach (var r in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role ?? "Student"));
            }

            // ✅ Create signing credentials using secret key
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            // ✅ Define token details
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),   // Token validity (7 days)
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"] ?? _config["JWT:Issuer"],
                Audience = _config["Jwt:Audience"] ?? _config["JWT:Audience"]
            };

            // ✅ Create and return token string
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

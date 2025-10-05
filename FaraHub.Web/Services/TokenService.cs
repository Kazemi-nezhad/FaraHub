using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FaraHub.Web.Models;
using Microsoft.Extensions.Configuration;

namespace FaraHub.Web.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(IConfiguration configuration, UserManager<AppUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public string GenerateToken(AppUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"];
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var userRoles = _userManager.GetRolesAsync(user).Result; // توجه: از await/async استفاده کنید در محیط‌هایی که ممکن است مشکل ایجاد کند، اما در این حالت ساده ممکن است قابل قبول باشد. برای اطمینان، متد GenerateToken را async کنید.

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Issued At
                // اضافه کردن نقش‌های کاربر به توکن
                //new Claim(ClaimTypes.Role, string.Join(",", userRoles)) // روش ساده برای یک نقش
            };

            // اضافه کردن نقش‌های کاربر به توکن (روش صحیح برای چندین نقش)
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
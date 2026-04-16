using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Purchases.Domain.Models.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace aspnet_mongo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;

        public AuthController(IOptions<JwtSettings> options)
        {
            _jwtSettings = options.Value;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var userIsValid = 
                request.Email == _jwtSettings.Email && 
                request.Password == _jwtSettings.Password;

            if (!userIsValid)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "123"),
                new Claim(JwtRegisteredClaimNames.Email, request.Email),
                new Claim(ClaimTypes.Role, "User"),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwt = jwtHandler.WriteToken(token);

            return Ok(new { accessToken = jwt });
        }

        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfoAsync()
        {
            await Task.Delay(500);

            return Ok("No info (yet)");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using mess_management.Models;
using mess_management.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace mess_management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtTokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext context, IJwtTokenService tokenService, ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) return Unauthorized(new { message = "Invalid email or password" });

            bool passwordMatches = false;
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                try
                {
                    passwordMatches = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error verifying password for user {UserId}", user.Id);
                    return Unauthorized(new { message = "Authentication failed" });
                }
            }

            if (!passwordMatches) return Unauthorized(new { message = "Invalid email or password" });

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Should match JwtSettings
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Should match JwtSettings
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName ?? "",
                    IsAdmin = user.IsAdmin == true
                }
            });
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (refreshToken == null || !refreshToken.IsActive)
                return Unauthorized(new { message = "Invalid or expired refresh token" });

            if (refreshToken.User == null)
                return Unauthorized(new { message = "User not found" });

            var newAccessToken = _tokenService.GenerateAccessToken(refreshToken.User);

            return Ok(new RefreshResponse
            {
                AccessToken = newAccessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }

        [HttpPost("revoke")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Revoke([FromBody] RevokeTokenRequest request)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (refreshToken == null) return NotFound(new { message = "Token not found" });

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            _context.Update(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Token revoked" });
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserInfo>> GetMe()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.AspNetUsers.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                IsAdmin = user.IsAdmin == true
            });
        }
    }
}

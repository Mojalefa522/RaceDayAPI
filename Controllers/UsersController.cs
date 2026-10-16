using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordService _passwordService;

        public UsersController(RaceDayDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                userId = user.UserID,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role.RoleName,
                createdAt = user.CreatedAt,
                isActive = user.IsActive
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Full name and email are required." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.UserID != userId);
            if (emailTaken)
            {
                return Conflict(new { message = "Email already in use." });
            }

            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim();

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("FullName", user.FullName);

            return Ok(new
            {
                userId = user.UserID,
                email = user.Email,
                fullName = user.FullName
            });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { message = "Old and new password are required." });
            }

            if (dto.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "New password must be at least 6 characters." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!_passwordService.VerifyPassword(dto.OldPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Old password is incorrect." });
            }

            user.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
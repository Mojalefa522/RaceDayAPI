using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordService _passwordService;

        public AuthController(RaceDayDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Role))
            {
                return BadRequest(new { message = "Email, password, full name and role are required." });
            }

            if (dto.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters." });
            }

            var roleName = dto.Role.Trim();
            if (roleName != "Organiser" && roleName != "Participant")
            {
                return BadRequest(new { message = "Role must be Organiser or Participant." });
            }

            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
            {
                return Conflict(new { message = "Email already registered." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role == null)
            {
                return BadRequest(new { message = "Role not configured in database." });
            }

            var user = new User
            {
                Email = dto.Email.Trim(),
                FullName = dto.FullName.Trim(),
                PasswordHash = _passwordService.HashPassword(dto.Password),
                RoleID = role.RoleID,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (roleName == "Organiser")
            {
                var profile = new OrganiserProfile
                {
                    UserID = user.UserID,
                    CompanyName = dto.CompanyName,
                    Website = dto.Website,
                    ContactPhone = dto.ContactPhone
                };
                _context.OrganiserProfiles.Add(profile);
            }
            else
            {
                var profile = new ParticipantProfile
                {
                    UserID = user.UserID,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    EmergencyContact = dto.EmergencyContact,
                    MedicalNotes = dto.MedicalNotes
                };
                _context.ParticipantProfiles.Add(profile);
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new
            {
                userId = user.UserID,
                email = user.Email,
                fullName = user.FullName,
                role = role.RoleName
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            HttpContext.Session.SetUserSession(user.UserID, user.Role.RoleName, user.FullName);

            return Ok(new
            {
                userId = user.UserID,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role.RoleName
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out." });
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            return Ok(new
            {
                userId = HttpContext.Session.GetUserId(),
                role = HttpContext.Session.GetRole(),
                fullName = HttpContext.Session.GetString("FullName")
            });
        }
    }
}
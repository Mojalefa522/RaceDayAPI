using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/participant/profile")]
    public class ParticipantProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Access denied. Participant role required." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var profile = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (profile == null)
            {
                return NotFound(new { message = "Participant profile not found." });
            }

            return Ok(new
            {
                profileId = profile.ProfileID,
                dateOfBirth = profile.DateOfBirth,
                gender = profile.Gender,
                emergencyContact = profile.EmergencyContact,
                medicalNotes = profile.MedicalNotes
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateParticipantProfileDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Access denied. Participant role required." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var profile = await _context.ParticipantProfiles
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (profile == null)
            {
                return NotFound(new { message = "Participant profile not found." });
            }

            profile.DateOfBirth = dto.DateOfBirth;
            profile.Gender = dto.Gender;
            profile.EmergencyContact = dto.EmergencyContact;
            profile.MedicalNotes = dto.MedicalNotes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                profileId = profile.ProfileID,
                dateOfBirth = profile.DateOfBirth,
                gender = profile.Gender,
                emergencyContact = profile.EmergencyContact,
                medicalNotes = profile.MedicalNotes
            });
        }
    }
}
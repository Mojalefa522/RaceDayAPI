using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/organiser/profile")]
    public class OrganiserProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganiserProfileController(RaceDayDbContext context)
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

            if (HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Access denied. Organiser role required." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var profile = await _context.OrganiserProfiles
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (profile == null)
            {
                return NotFound(new { message = "Organiser profile not found." });
            }

            return Ok(new
            {
                profileId = profile.ProfileID,
                companyName = profile.CompanyName,
                website = profile.Website,
                contactPhone = profile.ContactPhone
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateOrganiserProfileDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Access denied. Organiser role required." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var profile = await _context.OrganiserProfiles
                .FirstOrDefaultAsync(p => p.UserID == userId);

            if (profile == null)
            {
                return NotFound(new { message = "Organiser profile not found." });
            }

            profile.CompanyName = dto.CompanyName;
            profile.Website = dto.Website;
            profile.ContactPhone = dto.ContactPhone;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                profileId = profile.ProfileID,
                companyName = profile.CompanyName,
                website = profile.Website,
                contactPhone = profile.ContactPhone
            });
        }
    }
}
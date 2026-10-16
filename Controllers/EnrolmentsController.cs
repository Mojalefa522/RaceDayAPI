using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using RaceDayAPI.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("enrolments")]
        public async Task<IActionResult> Enrol([FromBody] CreateEnrolmentDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Only participants can enrol." });
            }

            var participantId = HttpContext.Session.GetUserId()!.Value;

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryId);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            if (category.CurrentEnrolments >= category.MaxCapacity)
            {
                return Conflict(new { message = "Category is full." });
            }

            var alreadyEnrolled = await _context.EventEnrolments
                .AnyAsync(e => e.CategoryID == dto.CategoryId && e.ParticipantID == participantId);

            if (alreadyEnrolled)
            {
                return Conflict(new { message = "You are already enrolled in this category." });
            }

            var enrolment = new EventEnrolment
            {
                CategoryID = dto.CategoryId,
                ParticipantID = participantId,
                EnrolmentDate = DateTime.Now,
                Status = "confirmed"
            };

            _context.EventEnrolments.Add(enrolment);
            category.CurrentEnrolments++;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                enrolmentId = enrolment.EnrolmentID,
                categoryId = enrolment.CategoryID,
                eventId = category.EventID,
                status = enrolment.Status
            });
        }

        [HttpGet("enrolments/my-enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Only participants can view their enrolments." });
            }

            var participantId = HttpContext.Session.GetUserId()!.Value;

            var enrolments = await _context.EventEnrolments
                .Where(e => e.ParticipantID == participantId)
                .Include(e => e.Category)
                    .ThenInclude(c => c.Event)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentID,
                    status = e.Status,
                    bibNumber = e.BibNumber,
                    enrolmentDate = e.EnrolmentDate,
                    category = new
                    {
                        categoryId = e.Category.CategoryID,
                        name = e.Category.Name,
                        distance = e.Category.Distance
                    },
                    eventDetails = new
                    {
                        eventId = e.Category.Event.EventID,
                        name = e.Category.Event.Name,
                        location = e.Category.Event.Location,
                        startDate = e.Category.Event.StartDate
                    }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpGet("events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Only organisers can view enrolments." });
            }

            var organiserId = HttpContext.Session.GetUserId()!.Value;

            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
            if (eventEntity == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventEntity.OrganiserID != organiserId)
            {
                return StatusCode(403, new { message = "You can only view enrolments for your own events." });
            }

            var enrolments = await _context.EventEnrolments
                .Where(e => e.Category.EventID == eventId)
                .Include(e => e.Category)
                .Include(e => e.Participant)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentID,
                    status = e.Status,
                    bibNumber = e.BibNumber,
                    enrolmentDate = e.EnrolmentDate,
                    category = new
                    {
                        categoryId = e.Category.CategoryID,
                        name = e.Category.Name,
                        distance = e.Category.Distance
                    },
                    participant = new
                    {
                        userId = e.Participant.UserID,
                        fullName = e.Participant.FullName,
                        email = e.Participant.Email
                    }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpDelete("enrolments/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Only participants can cancel enrolments." });
            }

            var participantId = HttpContext.Session.GetUserId()!.Value;

            var enrolment = await _context.EventEnrolments
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (enrolment.ParticipantID != participantId)
            {
                return StatusCode(403, new { message = "You can only cancel your own enrolments." });
            }

            if (enrolment.Category != null && enrolment.Category.CurrentEnrolments > 0)
            {
                enrolment.Category.CurrentEnrolments--;
            }

            _context.EventEnrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
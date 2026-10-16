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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("results")]
        public async Task<IActionResult> EnterResult([FromBody] EnterResultDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Only organisers can enter results." });
            }

            var organiserId = HttpContext.Session.GetUserId()!.Value;

            var enrolment = await _context.EventEnrolments
                .Include(e => e.Category)
                    .ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == dto.EnrolmentId);

            if (enrolment == null)
            {
                return NotFound(new { message = "Enrolment not found." });
            }

            if (enrolment.Category.Event.OrganiserID != organiserId)
            {
                return StatusCode(403, new { message = "You can only enter results for your own events." });
            }

            var result = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentID == dto.EnrolmentId);

            if (result == null)
            {
                result = new Result
                {
                    EnrolmentID = dto.EnrolmentId,
                    FinishTime = dto.FinishTime,
                    Position = dto.Position,
                    Notes = dto.Notes,
                    Status = "entered"
                };
                _context.Results.Add(result);
            }
            else
            {
                result.FinishTime = dto.FinishTime;
                result.Position = dto.Position;
                result.Notes = dto.Notes;
                result.Status = "updated";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                resultId = result.ResultID,
                enrolmentId = result.EnrolmentID,
                finishTime = result.FinishTime,
                position = result.Position,
                status = result.Status
            });
        }

        [HttpGet("results/my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Only participants can view their results." });
            }

            var participantId = HttpContext.Session.GetUserId()!.Value;

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                        .ThenInclude(c => c.Event)
                .Where(r => r.Enrolment.ParticipantID == participantId)
                .Select(r => new
                {
                    resultId = r.ResultID,
                    finishTime = r.FinishTime,
                    position = r.Position,
                    status = r.Status,
                    notes = r.Notes,
                    eventDetails = new
                    {
                        eventId = r.Enrolment.Category.Event.EventID,
                        name = r.Enrolment.Category.Event.Name,
                        location = r.Enrolment.Category.Event.Location
                    },
                    category = new
                    {
                        categoryId = r.Enrolment.Category.CategoryID,
                        name = r.Enrolment.Category.Name,
                        distance = r.Enrolment.Category.Distance
                    }
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("events/{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Only organisers can view event results." });
            }

            var organiserId = HttpContext.Session.GetUserId()!.Value;

            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
            if (eventEntity == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventEntity.OrganiserID != organiserId)
            {
                return StatusCode(403, new { message = "You can only view results for your own events." });
            }

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .Where(r => r.Enrolment.Category.EventID == eventId)
                .Select(r => new
                {
                    resultId = r.ResultID,
                    finishTime = r.FinishTime,
                    position = r.Position,
                    status = r.Status,
                    participant = new
                    {
                        userId = r.Enrolment.Participant.UserID,
                        fullName = r.Enrolment.Participant.FullName,
                        email = r.Enrolment.Participant.Email
                    },
                    category = new
                    {
                        categoryId = r.Enrolment.Category.CategoryID,
                        name = r.Enrolment.Category.Name
                    }
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}
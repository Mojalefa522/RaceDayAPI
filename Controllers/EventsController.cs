using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using RaceDayAPI.Services;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    eventId = e.EventID,
                    name = e.Name,
                    description = e.Description,
                    location = e.Location,
                    startDate = e.StartDate,
                    endDate = e.EndDate,
                    status = e.Status,
                    organiserId = e.OrganiserID
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventEntity == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            var categories = await _context.Categories
                .Where(c => c.EventID == id)
                .Select(c => new
                {
                    categoryId = c.CategoryID,
                    name = c.Name,
                    description = c.Description,
                    distance = c.Distance,
                    minAge = c.MinAge,
                    maxAge = c.MaxAge,
                    maxCapacity = c.MaxCapacity,
                    currentEnrolments = c.CurrentEnrolments
                })
                .ToListAsync();

            return Ok(new
            {
                eventId = eventEntity.EventID,
                name = eventEntity.Name,
                description = eventEntity.Description,
                location = eventEntity.Location,
                startDate = eventEntity.StartDate,
                endDate = eventEntity.EndDate,
                status = eventEntity.Status,
                organiser = new
                {
                    userId = eventEntity.Organiser.UserID,
                    fullName = eventEntity.Organiser.FullName,
                    email = eventEntity.Organiser.Email
                },
                categories
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateEventDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
                return Unauthorized(new { message = "Not logged in." });

            if (HttpContext.Session.GetRole() != "Organiser")
                return StatusCode(403, new { message = "Access denied. Organiser role required." });

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Location))
                return BadRequest(new { message = "Name and location are required." });

            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new { message = "End date must be after start date." });

            string? imageUrl = null;

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/uploads/{uniqueFileName}";
            }

            var userId = HttpContext.Session.GetUserId().Value;

            var newEvent = new Event
            {
                OrganiserID = userId,
                Name = dto.Name.Trim(),
                Description = dto.Description ?? "",
                Location = dto.Location.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "draft",
                CreatedAt = DateTime.Now,
                ImageUrl = imageUrl
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newEvent.EventID }, new
            {
                eventId = newEvent.EventID,
                name = newEvent.Name,
                status = newEvent.Status
            });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
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
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if (eventEntity == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventEntity.OrganiserID != userId)
            {
                return StatusCode(403, new { message = "You can only edit your own events." });
            }

            if (dto.EndDate <= dto.StartDate)
            {
                return BadRequest(new { message = "End date must be after start date." });
            }

            eventEntity.Name = dto.Name ?? eventEntity.Name;
            eventEntity.Description = dto.Description ?? eventEntity.Description;
            eventEntity.Location = dto.Location ?? eventEntity.Location;
            eventEntity.StartDate = dto.StartDate;
            eventEntity.EndDate = dto.EndDate;
            eventEntity.Status = dto.Status ?? eventEntity.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                eventId = eventEntity.EventID,
                name = eventEntity.Name,
                status = eventEntity.Status
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == id);

            if (eventEntity == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventEntity.OrganiserID != userId)
            {
                return StatusCode(403, new { message = "You can only delete your own events." });
            }

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{eventId}/enrol")]
        public async Task<IActionResult> Enrol(int eventId, [FromBody] CreateEnrolmentDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Only Participants can enrol." });
            }

            var userId = HttpContext.Session.GetUserId().Value;

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryId && c.EventID == eventId);

            if (category == null)
            {
                return NotFound(new { message = "Category not found for this event." });
            }

            if (category.CurrentEnrolments >= category.MaxCapacity)
            {
                return Conflict(new { message = "This category is full." });
            }

            var alreadyEnrolled = await _context.EventEnrolments.AnyAsync(e => e.CategoryID == dto.CategoryId && e.ParticipantID == userId);
            if (alreadyEnrolled)
            {
                return Conflict(new { message = "You are already enrolled in this category." });
            }

            var totalEnrolments = await _context.EventEnrolments.CountAsync();
            var bibNumber = $"B{totalEnrolments + 1:D3}";

            var enrolment = new EventEnrolment
            {
                CategoryID = dto.CategoryId,
                ParticipantID = userId,
                EnrolmentDate = DateTime.Now,
                Status = "pending",
                BibNumber = bibNumber
            };

            _context.EventEnrolments.Add(enrolment);
            category.CurrentEnrolments++;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Successfully enrolled." });
        }

        [HttpGet("~/api/enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Access denied." });
            }

            var userId = HttpContext.Session.GetUserId().Value;

            var enrolments = await _context.EventEnrolments
                .Include(e => e.Category)
                .ThenInclude(c => c.Event)
                .Where(e => e.ParticipantID == userId)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentID,
                    eventName = e.Category.Event.Name,
                    categoryName = e.Category.Name,
                    enrolmentDate = e.EnrolmentDate,
                    status = e.Status,
                    bibNumber = e.BibNumber
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpGet("~/api/my-results")]
        public async Task<IActionResult> GetMyResults()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Participant")
            {
                return StatusCode(403, new { message = "Access denied." });
            }

            var userId = HttpContext.Session.GetUserId().Value;

            var results = await _context.Results
                .Include(r => r.Enrolment)
                .ThenInclude(e => e.Category)
                .ThenInclude(c => c.Event)
                .Where(r => r.Enrolment.ParticipantID == userId)
                .Select(r => new
                {
                    eventName = r.Enrolment.Category.Event.Name,
                    categoryName = r.Enrolment.Category.Name,
                    finishTime = r.FinishTime.ToString(),
                    position = r.Position,
                    status = r.Status
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("{eventId}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
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
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventEntity == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventEntity.OrganiserID != userId)
            {
                return StatusCode(403, new { message = "You can only view enrolments for your own events." });
            }

            var enrolments = await _context.EventEnrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.Category.EventID == eventId)
                .Select(e => new
                {
                    enrolmentId = e.EnrolmentID,
                    participantName = e.Participant.FullName,
                    categoryName = e.Category.Name,
                    status = e.Status,
                    bibNumber = e.BibNumber,
                    enrolmentDate = e.EnrolmentDate
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        [HttpGet("{eventId}/results")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            if (!HttpContext.Session.IsLoggedIn() || HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Access denied." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventEntity == null) return NotFound(new { message = "Event not found." });
            if (eventEntity.OrganiserID != userId) return StatusCode(403, new { message = "You can only view results for your own events." });

            var enrolments = await (from e in _context.EventEnrolments
                                    join c in _context.Categories on e.CategoryID equals c.CategoryID
                                    join u in _context.Users on e.ParticipantID equals u.UserID
                                    join r in _context.Results on e.EnrolmentID equals r.EnrolmentID into results
                                    from res in results.DefaultIfEmpty()
                                    where c.EventID == eventId
                                    select new
                                    {
                                        enrolmentId = e.EnrolmentID,
                                        participantName = u.FullName,
                                        categoryName = c.Name,
                                        bibNumber = e.BibNumber,
                                        finishTime = res != null ? res.FinishTime.ToString() : null,
                                        position = res != null ? res.Position : (int?)null,
                                        status = res != null ? res.Status : "not entered"
                                    }).ToListAsync();

            return Ok(enrolments);
        }

        [HttpPost("{eventId}/results")]
        public async Task<IActionResult> SubmitResult(int eventId, [FromBody] SubmitResultDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn() || HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Access denied." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var enrolment = await _context.EventEnrolments
                .Include(e => e.Category)
                .ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == dto.EnrolmentId && e.Category.EventID == eventId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Category.Event.OrganiserID != userId) return StatusCode(403, new { message = "Access denied." });

            if (!TimeSpan.TryParse(dto.FinishTime, out TimeSpan parsedTime))
            {
                return BadRequest(new { message = "Invalid time format. Please use HH:MM:SS." });
            }

            var existingResult = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentID == dto.EnrolmentId);

            if (existingResult != null)
            {
                existingResult.FinishTime = parsedTime;
                existingResult.Position = dto.Position;
                existingResult.Status = "completed";
                existingResult.Notes = existingResult.Notes ?? "None";
            }
            else
            {
                var result = new Result
                {
                    EnrolmentID = dto.EnrolmentId,
                    FinishTime = parsedTime,
                    Position = dto.Position,
                    Status = "completed",
                    Notes = "None"
                };
                _context.Results.Add(result);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Result saved." });
        }
    }
}
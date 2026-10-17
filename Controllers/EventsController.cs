using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;
using RaceDayAPI.Models;
using RaceDayAPI.Services;

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

        /// <summary>
        /// Retrieves a list of all events.
        /// </summary>
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

        /// <summary>
        /// Retrieves a specific event by its ID, including its categories and organiser details.
        /// </summary>
        /// <param name="id">The ID of the event.</param>
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

        /// <summary>
        /// Creates a new event. Only accessible by Organisers.
        /// </summary>
        /// <param name="dto">The event details.</param>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Access denied. Organiser role required." });
            }

            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Location))
            {
                return BadRequest(new { message = "Name and location are required." });
            }

            if (dto.EndDate <= dto.StartDate)
            {
                return BadRequest(new { message = "End date must be after start date." });
            }

            var userId = HttpContext.Session.GetUserId().Value;

            var newEvent = new Event
            {
                OrganiserID = userId,
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Location = dto.Location.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "draft",
                CreatedAt = DateTime.Now
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

        /// <summary>
        /// Updates an existing event. Only accessible by the Organiser who created it.
        /// </summary>
        /// <param name="id">The ID of the event to update.</param>
        /// <param name="dto">The updated event details.</param>
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

        /// <summary>
        /// Deletes an event. Only accessible by the Organiser who created it.
        /// </summary>
        /// <param name="id">The ID of the event to delete.</param>
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
    }
}
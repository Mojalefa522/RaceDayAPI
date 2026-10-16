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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("events/{eventId}/categories")]
        public async Task<IActionResult> Create(int eventId, [FromBody] CreateCategoryDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Access denied. Organiser role required." });
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Category name is required." });
            }

            if (dto.MaxCapacity <= 0)
            {
                return BadRequest(new { message = "Max capacity must be greater than zero." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            var eventEntity = await _context.Events.FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventEntity == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (eventEntity.OrganiserID != userId)
            {
                return StatusCode(403, new { message = "You can only manage categories for your own events." });
            }

            var category = new Category
            {
                EventID = eventId,
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Distance = dto.Distance,
                MinAge = dto.MinAge,
                MaxAge = dto.MaxAge,
                MaxCapacity = dto.MaxCapacity,
                CurrentEnrolments = 0
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.CategoryID }, new
            {
                categoryId = category.CategoryID,
                name = category.Name,
                distance = category.Distance,
                maxCapacity = category.MaxCapacity
            });
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            return Ok(new
            {
                categoryId = category.CategoryID,
                eventId = category.EventID,
                name = category.Name,
                description = category.Description,
                distance = category.Distance,
                minAge = category.MinAge,
                maxAge = category.MaxAge,
                maxCapacity = category.MaxCapacity,
                currentEnrolments = category.CurrentEnrolments
            });
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                return Unauthorized(new { message = "Not logged in." });
            }

            if (HttpContext.Session.GetRole() != "Organiser")
            {
                return StatusCode(403, new { message = "Access denied. Organiser role required." });
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            if (category.Event.OrganiserID != userId)
            {
                return StatusCode(403, new { message = "You can only manage categories for your own events." });
            }

            category.Name = dto.Name ?? category.Name;
            category.Description = dto.Description ?? category.Description;
            category.Distance = dto.Distance;
            category.MinAge = dto.MinAge;
            category.MaxAge = dto.MaxAge;
            category.MaxCapacity = dto.MaxCapacity;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                categoryId = category.CategoryID,
                name = category.Name,
                distance = category.Distance,
                maxCapacity = category.MaxCapacity
            });
        }

        [HttpDelete("categories/{id}")]
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

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found." });
            }

            var userId = HttpContext.Session.GetUserId().Value;
            if (category.Event.OrganiserID != userId)
            {
                return StatusCode(403, new { message = "You can only manage categories for your own events." });
            }

            var hasEnrolments = await _context.EventEnrolments.AnyAsync(e => e.CategoryID == id);
            if (hasEnrolments)
            {
                return Conflict(new { message = "Cannot delete a category that has enrolments." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
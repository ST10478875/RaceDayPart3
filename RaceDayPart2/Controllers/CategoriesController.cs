
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        // Retrieve all categories and their associated events.
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Event)
                .OrderBy(c => c.EventId)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/event/5
        // Retrieve categories belonging to a specific event.
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetEventCategories(int eventId)
        {
            // Check whether the event exists.
            bool eventExists = await _context.Events
                .AnyAsync(e => e.Id == eventId);

            if (!eventExists)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Return only categories linked to the selected event.
            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/Categories
        // Create a new category for an existing event.
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryRequestDto dto)
        {
            // Check whether the associated event exists.
            var raceEvent = await _context.Events
                .FindAsync(dto.EventId);

            if (raceEvent == null)
            {
                return BadRequest(new
                {
                    message = "Associated event not found."
                });
            }

            string categoryName = dto.Name.Trim();

            // Prevent duplicate category names within the same event.
            bool categoryExists = await _context.Categories.AnyAsync(c =>
                c.EventId == dto.EventId &&
                c.Name.ToLower() == categoryName.ToLower());

            if (categoryExists)
            {
                return Conflict(new
                {
                    message = "This category already exists for the selected event."
                });
            }

            // Create the new category.
            var category = new Category
            {
                Name = categoryName,
                EventId = dto.EventId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventCategories),
                new { eventId = category.EventId },
                new
                {
                    message = "Category created successfully.",
                    categoryId = category.Id,
                    category.Name,
                    category.EventId
                });
        }

        // PUT: api/Categories/5
        // Update an existing category name.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(
            int id, [FromBody] CategoryUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            string categoryName = dto.Name.Trim();

            // Check that another category does not already use this name
            // within the same event.
            bool duplicateExists = await _context.Categories.AnyAsync(c =>
                c.EventId == category.EventId &&
                c.Id != id &&
                c.Name.ToLower() == categoryName.ToLower());

            if (duplicateExists)
            {
                return Conflict(new
                {
                    message = "Another category already uses this name."
                });
            }

            // Update the category name.
            category.Name = categoryName;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully.",
                categoryId = category.Id,
                category.Name
            });
        }

        // DELETE: api/Categories/5
        // Delete a category if it is not being used by an enrolment.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            // Check whether participants are enrolled in this category.
            bool hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.CategoryId == id);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message = "This category cannot be deleted because participants are enrolled in it."
                });
            }

            // Remove the category from the database.
            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }

    // Information required when creating a category.
    public class CategoryRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int EventId { get; set; }
    }

    // Information required when updating a category.
    public class CategoryUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
    }
}

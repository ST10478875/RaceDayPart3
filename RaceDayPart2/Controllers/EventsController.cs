
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Retrieve all events for the event catalogue.
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/organiser-events
        // Retrieve events, optionally filtered by organiser ID.
        [HttpGet("organiser-events")]
        public async Task<IActionResult> GetOrganiserEvents(
            [FromQuery] int? organiserId)
        {
            var query = _context.Events
                .Include(e => e.Categories)
                .AsQueryable();

            // Filter events when an organiser ID is supplied.
            if (organiserId.HasValue)
            {
                query = query.Where(e => e.OrganiserId == organiserId.Value);
            }

            var events = await query
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/5
        // Retrieve details of one event using its ID.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var raceEvent = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return Ok(raceEvent);
        }

        // POST: api/Events
        // Create a new event.
        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            [FromBody] EventRequestDto dto)
        {
            // Check whether the organiser exists.
            var organiserExists = await _context.Users
                .AnyAsync(u => u.Id == dto.OrganiserId);

            if (!organiserExists)
            {
                return BadRequest(new
                {
                    message = "The selected organiser does not exist."
                });
            }

            // Create the event using the supplied information.
            var raceEvent = new Event
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Date = dto.Date,
                Location = dto.Location.Trim(),
                Distance = dto.Distance,
                EventType = dto.EventType.Trim(),
                OrganiserId = dto.OrganiserId
            };

            // Add and save the new event.
            _context.Events.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.Id },
                raceEvent);
        }

        // PUT: api/Events/5
        // Update an existing event.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(
            int id, [FromBody] EventRequestDto dto)
        {
            // Find the event that must be updated.
            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Check whether the new organiser exists.
            var organiserExists = await _context.Users
                .AnyAsync(u => u.Id == dto.OrganiserId);

            if (!organiserExists)
            {
                return BadRequest(new
                {
                    message = "The selected organiser does not exist."
                });
            }

            // Update the event information.
            raceEvent.Name = dto.Name.Trim();
            raceEvent.Description = dto.Description.Trim();
            raceEvent.Date = dto.Date;
            raceEvent.Location = dto.Location.Trim();
            raceEvent.Distance = dto.Distance;
            raceEvent.EventType = dto.EventType.Trim();
            raceEvent.OrganiserId = dto.OrganiserId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                eventId = raceEvent.Id
            });
        }

        // DELETE: api/Events/5
        // Delete an event if it has no existing enrolments.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var raceEvent = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Prevent deletion when participants are already enrolled.
            bool hasEnrolments = await _context.Enrolments
                .AnyAsync(e => e.EventId == id);

            if (hasEnrolments)
            {
                return Conflict(new
                {
                    message = "This event cannot be deleted because participants are enrolled."
                });
            }

            // Remove the event's categories before deleting the event.
            _context.Categories.RemoveRange(raceEvent.Categories);

            // Remove the event.
            _context.Events.Remove(raceEvent);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }
    }

    // Data required when creating or updating an event.
    public class EventRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal Distance { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int OrganiserId { get; set; }
    }
}

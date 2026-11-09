
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Retrieve all enrolments with participant, event, category and result details.
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            var enrolments = await _context.Enrolments
                .Include(e => e.User)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .OrderByDescending(e => e.EnrolmentDate)
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/my-enrolments?email=runner@example.com
        // Retrieve enrolments belonging to a specific participant.
        [HttpGet("my-enrolments")]
        public async Task<IActionResult> GetMyEnrolments(
            [FromQuery] string email)
        {
            // Check that an email address was supplied.
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new
                {
                    message = "Email address is required."
                });
            }

            // Find the participant using their email address.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.Trim());

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // Retrieve only this participant's enrolments.
            var enrolments = await _context.Enrolments
                .Where(e => e.UserId == user.Id)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .OrderByDescending(e => e.EnrolmentDate)
                .Select(e => new
                {
                    e.Id,
                    e.EnrolmentDate,
                    e.UserId,
                    ParticipantEmail = user.Email,
                    Event = e.Event == null ? null : new
                    {
                        e.Event.Id,
                        e.Event.Name,
                        e.Event.Date,
                        e.Event.Location
                    },
                    Category = e.Category == null ? null : new
                    {
                        e.Category.Id,
                        e.Category.Name
                    },
                    Result = e.Result == null ? null : new
                    {
                        e.Result.FinishTime,
                        e.Result.Position
                    }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/Enrolments
        // Register a participant for an event and selected category.
        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] CreateEnrolmentDto dto)
        {
            // Check whether the participant exists.
            var user = await _context.Users.FindAsync(dto.UserId);

            if (user == null)
            {
                return BadRequest(new
                {
                    message = "Participant not found."
                });
            }

            // Only participant accounts may enrol in events.
            if (!string.Equals(user.Role, "Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only participants can enrol in events."
                });
            }

            // Check whether the event exists.
            var raceEvent = await _context.Events.FindAsync(dto.EventId);

            if (raceEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            // Prevent enrolment in an event that has already taken place.
            if (raceEvent.Date <= DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    message = "Enrolment is closed because this event has already started or taken place."
                });
            }

            // Check whether the selected category exists.
            var category = await _context.Categories.FindAsync(dto.CategoryId);

            if (category == null || category.EventId != dto.EventId)
            {
                return BadRequest(new
                {
                    message = "The selected category does not belong to this event."
                });
            }

            // Prevent the same participant from enrolling twice
            // in the same event.
            bool alreadyEnrolled = await _context.Enrolments.AnyAsync(e =>
                e.UserId == dto.UserId &&
                e.EventId == dto.EventId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this event."
                });
            }

            // Create the new enrolment.
            var enrolment = new Enrolment
            {
                UserId = dto.UserId,
                EventId = dto.EventId,
                CategoryId = dto.CategoryId,
                EnrolmentDate = DateTime.UtcNow
            };

            // Save the enrolment to the database.
            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMyEnrolments),
                new { email = user.Email },
                new
                {
                    message = "Enrolment successful!",
                    enrolmentId = enrolment.Id,
                    userId = enrolment.UserId,
                    eventId = enrolment.EventId,
                    categoryId = enrolment.CategoryId,
                    enrolmentDate = enrolment.EnrolmentDate
                });
        }
    }

    // Information required to enrol a participant in an event.
    public class CreateEnrolmentDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int EventId { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}

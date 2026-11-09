
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // Retrieve all race results for the organiser.
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.User)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/my-results?email=runner@example.com
        // Retrieve results belonging to one participant.
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults([FromQuery] string email)
        {
            // Check that an email address was supplied.
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new
                {
                    message = "Email address is required."
                });
            }

            // Find the participant.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.Trim());

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // Retrieve only results linked to this participant's enrolments.
            var results = await _context.Results
                .Where(r => r.Enrolment.UserId == user.Id)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .Select(r => new
                {
                    r.Id,
                    r.FinishTime,
                    r.Position,
                    Event = new
                    {
                        r.Enrolment.Event.Id,
                        r.Enrolment.Event.Name,
                        r.Enrolment.Event.Date,
                        r.Enrolment.Event.Location
                    },
                    Category = new
                    {
                        r.Enrolment.Category.Id,
                        r.Enrolment.Category.Name
                    }
                })
                .OrderBy(r => r.Position)
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/Results
        // Create a result or update an existing result for an enrolment.
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateResult(
            [FromBody] CreateResultDto dto)
        {
            // Validate the finishing time.
            if (dto.FinishTime <= TimeSpan.Zero)
            {
                return BadRequest(new
                {
                    message = "Finish time must be greater than zero."
                });
            }

            // Validate the finishing position.
            if (dto.Position < 1)
            {
                return BadRequest(new
                {
                    message = "Position must be at least 1."
                });
            }

            // Find the enrolment and its existing result.
            var enrolment = await _context.Enrolments
                .Include(e => e.Result)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrolmentId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment record not found."
                });
            }

            int resultId;

            // Update the result if one already exists.
            if (enrolment.Result != null)
            {
                enrolment.Result.FinishTime = dto.FinishTime;
                enrolment.Result.Position = dto.Position;

                resultId = enrolment.Result.Id;
            }
            else
            {
                // Otherwise, create a new result.
                var result = new Result
                {
                    EnrolmentId = dto.EnrolmentId,
                    FinishTime = dto.FinishTime,
                    Position = dto.Position
                };

                _context.Results.Add(result);

                // Save first so the database generates the result ID.
                await _context.SaveChangesAsync();

                resultId = result.Id;

                return Ok(new
                {
                    message = "Race result saved successfully!",
                    resultId = resultId
                });
            }

            // Save the updated result.
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Race result updated successfully!",
                resultId = resultId
            });
        }
    }

    // Data required when recording a participant's race result.
    public class CreateResultDto
    {
        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }

        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int Position { get; set; }
    }
}

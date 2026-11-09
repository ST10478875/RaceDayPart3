
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly BlobContainerClient _containerClient;

        // Receive the database and Azure container through dependency injection.
        public UserController(
            RaceDayDbContext context,
            BlobContainerClient containerClient)
        {
            _context = context;
            _containerClient = containerClient;
        }

        // GET: api/User/profile?email=runner@example.com
        // Retrieve a participant's profile information.
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.Trim());

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Return profile information without exposing the password hash.
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.ProfilePictureUrl
            });
        }

        // PUT: api/User/profile
        // Update a participant's username and email address.
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentEmail) ||
                string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.NewEmail))
            {
                return BadRequest(new
                {
                    message = "All profile fields are required."
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(
                u => u.Email == dto.CurrentEmail.Trim());

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Check whether another account already uses the new email.
            bool emailExists = await _context.Users.AnyAsync(u =>
                u.Email == dto.NewEmail.Trim() && u.Id != user.Id);

            if (emailExists)
            {
                return Conflict(new
                {
                    message = "That email address is already registered."
                });
            }

            // Update the participant's profile details.
            user.Username = dto.Username.Trim();
            user.Email = dto.NewEmail.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully!",
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                user.ProfilePictureUrl
            });
        }

        // POST: api/User/profile-picture
        // Upload a participant's profile picture to Azure Blob Storage.
        [HttpPost("profile-picture")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(
            [FromForm] string email,
            IFormFile file)
        {
            // Check that an email was supplied.
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            // Check that a file was supplied.
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Please select an image." });
            }

            // Limit profile pictures to 5 MB.
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    message = "The image must not exceed 5 MB."
                });
            }

            // Allow only common image formats.
            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/webp"
            };

            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(new
                {
                    message = "Only JPG, PNG, GIF and WEBP images are allowed."
                });
            }

            // Find the participant in the database.
            var user = await _context.Users.FirstOrDefaultAsync(
                u => u.Email == email.Trim());

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Check that the file has an allowed extension.
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    message = "Invalid image file extension."
                });
            }

            // Generate a unique file name to avoid overwriting other images.
            var fileName = $"profile-{user.Id}-{Guid.NewGuid()}{extension}";

            // Create the Azure blob reference.
            var blobClient = _containerClient.GetBlobClient(fileName);

            // Upload the image to Azure Blob Storage.
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(
                    stream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = file.ContentType
                        }
                    });
            }

            // Save the uploaded image URL in the user's profile.
            user.ProfilePictureUrl = blobClient.Uri.ToString();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile picture uploaded successfully!",
                profilePictureUrl = user.ProfilePictureUrl
            });
        }
    }

    // Information required when updating a participant's profile.
    public class UpdateProfileDto
    {
        [Required]
        public string CurrentEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = string.Empty;
    }
}

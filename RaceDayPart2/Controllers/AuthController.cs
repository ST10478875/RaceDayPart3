
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.models;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        // Used to securely hash passwords and verify login passwords.
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Register a new user account.
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Check that the email address is not already registered.
            string email = dto.Email.Trim();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                return Conflict(new
                {
                    message = "This email address is already registered."
                });
            }

            // Public registration is restricted to Participant accounts.
            // Organiser accounts should be created through an authorised process.
            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = email,
                Role = "Participant"
            };

            // Hash the password before storing it in the database.
            user.PasswordHash = _passwordHasher.HashPassword(
                user, dto.Password);

            // Save the new account.
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful!",
                userId = user.Id,
                email = user.Email,
                role = user.Role
            });
        }

        // Authenticate a user using their email address and password.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Find the account using the supplied email address.
            string email = dto.Email.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            // Do not reveal whether an email address exists.
            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Verify the supplied password against the stored password hash.
            var verificationResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

            // Reject the login if the password is incorrect.
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Return the actual user's details to the MVC application.
            return Ok(new
            {
                message = "Login successful!",
                userId = user.Id,
                email = user.Email,
                username = user.Username,
                role = user.Role
            });
        }

        // Retrieve a user's profile using their email address.
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            // Check that an email address was supplied.
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new
                {
                    message = "Email address is required."
                });
            }

            // Find the user in the database.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email.Trim());

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // Return profile information without exposing the password hash.
            return Ok(new
            {
                userId = user.Id,
                username = user.Username ?? "",
                email = user.Email ?? "",
                role = user.Role ?? "Participant"
            });
        }
    }

    // Data required when registering a new account.
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        // New public registrations are participants by default.
        public string Role { get; set; } = "Participant";
    }

    // Data required when logging into an account.
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

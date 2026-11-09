
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        // Used to create HTTP requests to the RaceDay API.
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor.
        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Displays the login page.
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Sends the user's login details to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Check that all required login fields are valid.
            if (!ModelState.IsValid)
                return View(model);

            // Create the named HTTP client configured in Program.cs.
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                // Send the login details to the API.
                var response = await client.PostAsJsonAsync(
                    "api/Auth/login", model);

                // Check whether the API accepted the login.
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    // Extract the user information returned by the API.
                    string email = GetJsonString(root, "email", "username");
                    string role = GetJsonString(root, "role");
                    string userId = GetJsonString(root, "userId");

                    // Read the username. Use the email if no username is returned.
                    string username = GetJsonString(root, "username", "name");

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        username = email;
                    }

                    // Make sure the API returned the required information.
                    if (string.IsNullOrWhiteSpace(email) ||
                        string.IsNullOrWhiteSpace(role) ||
                        string.IsNullOrWhiteSpace(userId))
                    {
                        ModelState.AddModelError("",
                            "The API did not return complete user information.");

                        return View(model);
                    }

                    // Store the logged-in user's details in the session.
                    HttpContext.Session.SetString("UserEmail", email);
                    HttpContext.Session.SetString("UserRole", role);
                    HttpContext.Session.SetString("UserId", userId);
                    HttpContext.Session.SetString("UserName", username);

                    // Redirect organisers to their dashboard.
                    if (role.Equals("Organiser",
                        StringComparison.OrdinalIgnoreCase) ||
                        role.Equals("Organizer",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Organiser");
                    }

                    // Redirect participants to the event catalogue.
                    if (role.Equals("Participant",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Catalog", "Participant");
                    }

                    // Handle an unexpected role returned by the API.
                    HttpContext.Session.Clear();

                    ModelState.AddModelError("",
                        "Your account has an unrecognised role.");

                    return View(model);
                }

                // Display a message when the login details are incorrect.
                ModelState.AddModelError("", "Invalid email or password.");

                return View(model);
            }
            catch (HttpRequestException)
            {
                // Display an error if the API cannot be reached.
                ModelState.AddModelError("",
                    "Cannot connect to the RaceDay API. Please check that it is running.");

                return View(model);
            }
            catch (JsonException)
            {
                // Handle an unexpected response from the API.
                ModelState.AddModelError("",
                    "The API returned an invalid response.");

                return View(model);
            }
        }

        // Displays the registration page.
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Sends new account details to the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Validate the registration information.
            if (!ModelState.IsValid)
                return View(model);

            // Create the named API client.
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            try
            {
                // Send registration details to the API.
                var response = await client.PostAsJsonAsync(
                    "api/Auth/register", model);

                // If registration succeeds, redirect to login.
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Registration successful. You can now log in.";

                    return RedirectToAction("Login");
                }

                // Read the actual error returned by the API.
                var errorMessage =
                    await response.Content.ReadAsStringAsync();

                // Display the API error to help identify the problem.
                ModelState.AddModelError("",
                    $"Registration failed. API response: {errorMessage}");

                return View(model);
            }
            catch (HttpRequestException)
            {
                // Handle API connection problems.
                ModelState.AddModelError("",
                    "Cannot connect to the RaceDay API. Please try again.");

                return View(model);
            }
            catch (JsonException)
            {
                // Handle an invalid JSON response.
                ModelState.AddModelError("",
                    "The API returned an invalid response.");

                return View(model);
            }
        }

        // Logs the user out by clearing all session information.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        // Reads a string property from JSON using alternative property names.
        private static string GetJsonString(
            JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var value))
                {
                    if (value.ValueKind == JsonValueKind.String)
                        return value.GetString() ?? "";

                    if (value.ValueKind == JsonValueKind.Number)
                        return value.ToString();
                }
            }

            return "";
        }
    }
}

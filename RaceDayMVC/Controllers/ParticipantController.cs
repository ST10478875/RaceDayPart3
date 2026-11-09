
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Display all available race events.
        [HttpGet]
        public async Task<IActionResult> Catalog()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");
                var response = await client.GetAsync("api/events");

                if (!response.IsSuccessStatusCode)
                    return View(new List<EventVm>());

                var json = await response.Content.ReadAsStringAsync();

                var events = JsonSerializer.Deserialize<List<EventVm>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return View(events ?? new List<EventVm>());
            }
            catch
            {
                return View(new List<EventVm>());
            }
        }

        // Display the race entry page and its available categories.
        [HttpGet]
        public async Task<IActionResult> EnterEvent(int eventId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) ||
                !string.Equals(role, "Participant",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                // Load the selected event.
                var eventsResponse = await client.GetAsync("api/events");

                if (!eventsResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Could not load race events.";
                    return RedirectToAction(nameof(Catalog));
                }

                var eventsJson = await eventsResponse.Content.ReadAsStringAsync();

                var events = JsonSerializer.Deserialize<List<EventVm>>(
                    eventsJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                var selectedEvent = events?.FirstOrDefault(e => e.Id == eventId);

                if (selectedEvent == null)
                {
                    TempData["ErrorMessage"] = "Race event not found.";
                    return RedirectToAction(nameof(Catalog));
                }

                // Load categories belonging to this event.
                var categoriesResponse = await client.GetAsync(
                    $"api/Categories/event/{eventId}");

                if (!categoriesResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Could not load race categories.";
                    return RedirectToAction(nameof(Catalog));
                }

                var categoriesJson =
                    await categoriesResponse.Content.ReadAsStringAsync();

                var categories =
                    JsonSerializer.Deserialize<List<CategoryOptionVm>>(
                        categoriesJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                var model = new EnrolmentCreateVm
                {
                    EventId = selectedEvent.Id,
                    EventName = selectedEvent.Name,
                    Categories = categories ?? new List<CategoryOptionVm>()
                };

                return View(model);
            }
            catch
            {
                TempData["ErrorMessage"] =
                    "An error occurred while loading the race entry page.";

                return RedirectToAction(nameof(Catalog));
            }
        }

        // Submit the participant's race entry.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterEvent(EnrolmentCreateVm model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId) ||
                !string.Equals(role, "Participant",
                    StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userId, out int participantId))
            {
                TempData["ErrorMessage"] = "Invalid participant account.";
                return RedirectToAction(nameof(Catalog));
            }

            if (model.EventId <= 0 || model.CategoryId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.CategoryId),
                    "Please select a race category.");

                return await ReloadEntryPage(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var enrolmentData = new
                {
                    UserId = participantId,
                    EventId = model.EventId,
                    CategoryId = model.CategoryId
                };

                var response = await client.PostAsJsonAsync(
                    "api/Enrolments",
                    enrolmentData);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "You have successfully entered the race!";

                    return RedirectToAction(nameof(MyEnrolments));
                }

                var error = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] =
                    $"Race entry failed: {error}";

                return RedirectToAction(nameof(Catalog));
            }
            catch
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the RaceDay API.";

                return RedirectToAction(nameof(Catalog));
            }
        }

        // Reload the entry page if validation fails.
        private async Task<IActionResult> ReloadEntryPage(
            EnrolmentCreateVm model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var eventsResponse = await client.GetAsync("api/events");

                if (eventsResponse.IsSuccessStatusCode)
                {
                    var eventsJson =
                        await eventsResponse.Content.ReadAsStringAsync();

                    var events = JsonSerializer.Deserialize<List<EventVm>>(
                        eventsJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    model.EventName =
                        events?.FirstOrDefault(e => e.Id == model.EventId)?.Name
                        ?? "Selected Race";
                }

                var categoriesResponse = await client.GetAsync(
                    $"api/Categories/event/{model.EventId}");

                if (categoriesResponse.IsSuccessStatusCode)
                {
                    var categoriesJson =
                        await categoriesResponse.Content.ReadAsStringAsync();

                    model.Categories =
                        JsonSerializer.Deserialize<List<CategoryOptionVm>>(
                            categoriesJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? new List<CategoryOptionVm>();
                }
            }
            catch
            {
                TempData["ErrorMessage"] =
                    "Could not reload race categories.";
            }

            return View("EnterEvent", model);
        }

        // Display the logged-in participant's enrolments.
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.GetAsync(
                    $"api/enrolments/my-enrolments?email={Uri.EscapeDataString(email)}");

                if (!response.IsSuccessStatusCode)
                    return View(new List<EnrolmentVm>());

                var json = await response.Content.ReadAsStringAsync();

                var enrolments = JsonSerializer.Deserialize<List<EnrolmentVm>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return View(enrolments ?? new List<EnrolmentVm>());
            }
            catch
            {
                return View(new List<EnrolmentVm>());
            }
        }

        // Display the logged-in participant's results.
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.GetAsync(
                    $"api/results/my-results?email={Uri.EscapeDataString(email)}");

                if (!response.IsSuccessStatusCode)
                    return View(new List<ResultVm>());

                var json = await response.Content.ReadAsStringAsync();

                var results = JsonSerializer.Deserialize<List<ResultVm>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return View(results ?? new List<ResultVm>());
            }
            catch
            {
                return View(new List<ResultVm>());
            }
        }

        // Display the logged-in participant's profile.
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.GetAsync(
                    $"api/User/profile?email={Uri.EscapeDataString(email)}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Could not load your profile.";

                    return View(new ProfileViewModel { Email = email });
                }

                var json = await response.Content.ReadAsStringAsync();

                var profile = JsonSerializer.Deserialize<ProfileViewModel>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return View(profile ?? new ProfileViewModel { Email = email });
            }
            catch
            {
                TempData["ErrorMessage"] = "The API could not be reached.";

                return View(new ProfileViewModel { Email = email });
            }
        }

        // Upload the participant's profile picture through the API.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            if (model.ProfilePictureFile == null ||
                model.ProfilePictureFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a picture first.";
                return RedirectToAction(nameof(Profile));
            }

            try
            {
                using var content = new MultipartFormDataContent();

                content.Add(new StringContent(email), "email");

                using var stream = model.ProfilePictureFile.OpenReadStream();
                using var fileContent = new StreamContent(stream);

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(model.ProfilePictureFile.ContentType);

                content.Add(
                    fileContent,
                    "file",
                    Path.GetFileName(model.ProfilePictureFile.FileName));

                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.PostAsync(
                    "api/User/profile-picture",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Your profile picture was uploaded successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "The picture could not be uploaded. Please try again.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] =
                    "An error occurred while uploading your picture.";
            }

            return RedirectToAction(nameof(Profile));
        }
    }
}

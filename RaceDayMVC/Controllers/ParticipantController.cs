
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Text.Json;
using System.Net.Http.Headers;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        // HTTP client factory used to communicate with the RaceDay API.
        private readonly IHttpClientFactory _httpClientFactory;

        // Constructor.
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

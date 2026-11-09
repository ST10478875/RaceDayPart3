
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Text.Json;
using System.Net.Http.Json;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OrganiserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Check whether the logged-in user is an organiser.
        private bool IsOrganiser()
        {
            var role = HttpContext.Session.GetString("UserRole");

            return role == "Organizer" || role == "Organiser";
        }

        // ORGANISER DASHBOARD
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");
                var response = await client.GetAsync("api/events/organiser-events");

                List<EventViewModel> events = new();

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    events = JsonSerializer.Deserialize<List<EventViewModel>>(
                        json, JsonOptions) ?? new List<EventViewModel>();
                }
                else
                {
                    TempData["ErrorMessage"] = "Could not load events.";
                }

                return View(events);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the RaceDay API.";

                return View(new List<EventViewModel>());
            }
        }

        // VIEW EVENT DETAILS
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");
                var response = await client.GetAsync($"api/events/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var ev = JsonSerializer.Deserialize<EventViewModel>(
                        json, JsonOptions);

                    if (ev != null)
                        return View(ev);
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Could not connect to the API.";
            }

            return RedirectToAction("Index");
        }

        // DISPLAY EDIT FORM
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");
                var response = await client.GetAsync($"api/events/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var ev = JsonSerializer.Deserialize<EventViewModel>(
                        json, JsonOptions);

                    if (ev != null)
                        return View(ev);
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Could not connect to the API.";
            }

            return RedirectToAction("Index");
        }

        // SAVE EVENT CHANGES
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.PutAsJsonAsync(
                    $"api/events/{model.Id}", model);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Event updated successfully!";

                    return RedirectToAction("Index");
                }

                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    "Failed to update event: " + error);

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Could not connect to the API.");

                return View(model);
            }
        }

        // DISPLAY CREATE FORM
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View(new EventViewModel
            {
                Date = DateTime.Now.AddDays(1)
            });
        }

        // CREATE EVENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetString("UserId");

            if (!int.TryParse(userId, out int organiserId))
            {
                ModelState.AddModelError(
                    "",
                    "Your organiser account could not be identified.");

                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var eventRequest = new
                {
                    model.Name,
                    model.Description,
                    model.Date,
                    model.Location,
                    Distance = (decimal)model.Distance,
                    model.EventType,
                    OrganiserId = organiserId
                };

                var response = await client.PostAsJsonAsync(
                    "api/events", eventRequest);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Event created successfully!";

                    return RedirectToAction("Index");
                }

                var error = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    "",
                    "Could not create event: " + error);

                return View(model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    "",
                    "Could not connect to the API.");

                return View(model);
            }
        }

        // DELETE EVENT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");
                var response = await client.DeleteAsync($"api/events/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Event deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Could not delete event. " +
                        await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the RaceDay API.";
            }

            return RedirectToAction("Index");
        }

        // VIEW CATEGORIES FOR AN EVENT
        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.GetAsync(
                    $"api/categories/event/{eventId}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Could not load categories.";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();

                var categories =
                    JsonSerializer.Deserialize<List<CategoryViewModel>>(
                        json, JsonOptions) ?? new List<CategoryViewModel>();

                ViewBag.EventId = eventId;

                return View(categories);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the API.";

                return RedirectToAction("Index");
            }
        }

        // CREATE CATEGORY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(
            CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid category name.";

                return RedirectToAction(
                    "Categories", new { eventId = model.EventId });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.PostAsJsonAsync(
                    "api/categories",
                    new { model.Name, model.EventId });

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Category created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the API.";
            }

            return RedirectToAction(
                "Categories", new { eventId = model.EventId });
        }

        // EDIT CATEGORY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(
            CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Please enter a valid category name.";

                return RedirectToAction(
                    "Categories", new { eventId = model.EventId });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.PutAsJsonAsync(
                    $"api/categories/{model.Id}",
                    new { model.Name });

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Category updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the API.";
            }

            return RedirectToAction(
                "Categories", new { eventId = model.EventId });
        }

        // DELETE CATEGORY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.DeleteAsync(
                    $"api/categories/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Category deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the API.";
            }

            return RedirectToAction(
                "Categories", new { eventId });
        }

        // DISPLAY RESULTS FOR A SELECTED EVENT
        [HttpGet]
        public async Task<IActionResult> Results(int eventId)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.GetAsync(
                    $"api/enrolments/event/{eventId}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Could not load event participants.";

                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Match API JSON property names to C# model properties,
                // regardless of uppercase or lowercase letters.
                var enrolments =
                    JsonSerializer.Deserialize<List<EnrolmentResultVm>>(
                        json, JsonOptions) ?? new List<EnrolmentResultVm>();

                var model = new OrganiserResultsPageVm
                {
                    EventId = eventId,
                    Enrolments = enrolments
                };

                if (enrolments.Any())
                {
                    model.EventName = enrolments.First().EventName;
                }
                else
                {
                    var eventResponse = await client.GetAsync(
                        $"api/events/{eventId}");

                    if (eventResponse.IsSuccessStatusCode)
                    {
                        var eventJson =
                            await eventResponse.Content.ReadAsStringAsync();

                        var raceEvent =
                            JsonSerializer.Deserialize<EventViewModel>(
                                eventJson, JsonOptions);

                        model.EventName = raceEvent?.Name ?? "Race Event";
                    }
                }

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Could not connect to the RaceDay API.";

                return RedirectToAction("Index");
            }
            catch (JsonException)
            {
                TempData["ErrorMessage"] =
                    "The results received from the API could not be read.";

                return RedirectToAction("Index");
            }
        }

        // SAVE OR UPDATE A PARTICIPANT'S RACE RESULT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(
            int eventId,
            int enrolmentId,
            TimeSpan finishTime,
            int position)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (finishTime <= TimeSpan.Zero || position < 1)
            {
                TempData["Error"] =
                    "Please enter a valid finishing time and position.";

                return RedirectToAction(
                    nameof(Results), new { eventId });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var result = new
                {
                    EnrolmentId = enrolmentId,
                    FinishTime = finishTime,
                    Position = position
                };

                var response = await client.PostAsJsonAsync(
                    "api/results", result);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] =
                        "Race result saved successfully!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        "Could not save result: " + error;
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Could not connect to the RaceDay API.";
            }

            return RedirectToAction(
                nameof(Results), new { eventId });
        }
    }
}

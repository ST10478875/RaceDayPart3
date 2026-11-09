using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using System.Diagnostics;

namespace RaceDayMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // If the user is logged in as an Organizer, send them to the Organiser dashboard
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Organizer")
            {
                return RedirectToAction("Index", "Organiser");
            }
            else if (role == "Participant")
            {
                return RedirectToAction("Catalog", "Participant");
            }

            // Otherwise show the landing/catalog view
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
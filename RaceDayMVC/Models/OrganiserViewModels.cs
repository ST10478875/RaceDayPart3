using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class EventViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required")]
        public double Distance { get; set; }

        [Required(ErrorMessage = "Event type is required")]
        public string EventType { get; set; } = string.Empty;

        public string? BannerImageUrl { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? BannerImageFile { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public int EventId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        public string Name { get; set; } = string.Empty;
    }

    public class ResultEntryViewModel
    {
        public int EnrolmentId { get; set; }

        [Required(ErrorMessage = "Finish time is required")]
        public string FinishTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Finishing position is required")]
        public int FinishingPosition { get; set; }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    // Stores event information for participant pages.
    public class EventVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
    }

    // Stores enrolment information for participant pages.
    public class EnrolmentVm
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }

    // Stores race result information for participant pages.
    public class ResultVm
    {
        public int Id { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public ResultEventVm? Event { get; set; }
        public ResultCategoryVm? Category { get; set; }
    }

    // Stores event details included in a race result.
    public class ResultEventVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    // Stores category details included in a race result.
    public class ResultCategoryVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Stores participant profile information.
    public class ProfileViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? ProfilePictureFile { get; set; }
    }

    // Stores race categories available for an event.
    public class CategoryOptionVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    // Stores information required to enter a race.
    public class EnrolmentCreateVm
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please select a race category.")]
        public int CategoryId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public List<CategoryOptionVm> Categories { get; set; } = new();
    }
}

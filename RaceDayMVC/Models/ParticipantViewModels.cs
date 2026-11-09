
using System;
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
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string TimeTaken { get; set; } = string.Empty;
    }

    // Stores participant profile information.
    public class ProfileViewModel
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }

        // Holds the image selected by the participant.
        [DataType(DataType.Upload)]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}

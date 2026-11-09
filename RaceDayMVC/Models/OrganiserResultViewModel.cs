
using System;
using System.Collections.Generic;

namespace RaceDayMVC.Models
{
    // Stores the information displayed on the organiser's results page.
    public class OrganiserResultsPageVm
    {
        // Identifies the race event being managed.
        public int EventId { get; set; }

        // Stores the name of the selected race event.
        public string EventName { get; set; } = string.Empty;

        // Stores all participants enrolled in this event.
        public List<EnrolmentResultVm> Enrolments { get; set; } = new();
    }

    // Stores enrolment and race result information for one participant.
    public class EnrolmentResultVm
    {
        // Unique ID of the participant's enrolment.
        public int EnrolmentId { get; set; }

        // Email address identifying the participant.
        public string ParticipantEmail { get; set; } = string.Empty;

        // Name of the race event.
        public string EventName { get; set; } = string.Empty;

        // Race category selected by the participant.
        public string CategoryName { get; set; } = string.Empty;

        // Participant's finishing time.
        // Nullable because a result may not have been recorded yet.
        public TimeSpan? FinishTime { get; set; }

        // Participant's finishing position.
        // Nullable because a result may not have been recorded yet.
        public int? Position { get; set; }
    }
}

namespace RaceDay.API.models
{//start of namespace 

    public class Event
    {//start of class

        //properties to hold or store the following
        // Primary Key for the Event entity
        public int Id { get; set; }

        // Name of the race event (e.g."Soweto Spring Marathon")
        public string Name { get; set; } = string.Empty;

        // Detailed description of the event, route, or guidelines
        public string Description { get; set; } = string.Empty;

        // Scheduled date and time of the event
        public DateTime Date { get; set; }

        // Physical location or starting venue of the event
        public string Location { get; set; } = string.Empty;

        // Total distance of the event in kilometers
        public decimal Distance { get; set; }

        // Type of discipline: "Run", "Walk", or "Cycle"
        public string EventType { get; set; } = string.Empty;

        // Foreign Key linking the event to its creator (Organiser)
        public int OrganiserId { get; set; }

        // Navigation property for the Organiser user
        public User Organiser { get; set; } = null!;

        // Navigation property: Categories available for this event
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        // Navigation property: Participant enrolments registered for this event
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();


    }//end of class
}//end of namespace
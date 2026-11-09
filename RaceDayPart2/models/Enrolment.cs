namespace RaceDay.API.models
{//start of the namespace
    
    public class Enrolment
    {//start of class

        // Primary Key for the Enrolment entity
        public int Id { get; set; }

        //  recording when the participant enrolled
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        // Foreign Key linking to the registered User (Participant)
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Foreign Key linking to the selected Event
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        // Foreign Key linking to the chosen Category within the event
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Optional navigation property: Official result recorded after event completion
        public Result? Result { get; set; }

    }//end of class
}//end of namespace
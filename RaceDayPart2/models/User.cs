namespace RaceDay.API.models
{//start of  namespace

    public class User
    {//start of user class

        //creating properties to store 

        public string? ProfilePictureUrl { get; set; }
        // Primary Key for the User entity
        public int Id { get; set; }

        // Unique username chosen by the user during registration
        public string Username { get; set; } = string.Empty;

        // User email address used for contact and login identification
        public string Email { get; set; } = string.Empty;

        // Securely hashed password never stored in plain text
        public string PasswordHash { get; set; } = string.Empty;

        // User role determining access permissions: "Organiser" or "Participant"
        public string Role { get; set; } = "Participant";

        // Navigation property: Enrolments associated with this participant
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

        // Navigation property: Events created and managed by this user (if Organiser)
        public ICollection<Event> Events { get; set; } = new List<Event>();

    }//end of class
}//end of namespace
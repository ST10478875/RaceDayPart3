namespace RaceDay.API.models
{//start of the namespace
    
    public class Category
    {//start of the class

        // Primary Key for the Category entity
        public int Id { get; set; }

        // Name/Title of the category
        public string Name { get; set; } = string.Empty;

        // Foreign Key linking the category to a specific event
        public int EventId { get; set; }

        // Navigation property for the parent Event
        public Event Event { get; set; } = null!;

        // Navigation property: Enrolments registered under this category
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    }//end of class
}//end of namespace
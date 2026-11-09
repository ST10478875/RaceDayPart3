namespace RaceDay.API.models
{//start of namespace
   
    public class Result
    {//start of class

        // Primary Key for the Result entity
        public int Id { get; set; }

        // Total elapsed finish time recorded for the participant
        public TimeSpan FinishTime { get; set; }

        // Official finishing position/place (e.g., 1st, 2nd, 3rd)
        public int Position { get; set; }

        // Foreign Key linking the result to a specific enrolment record
        public int EnrolmentId { get; set; }

        // Navigation property for the corresponding Enrolment
        public Enrolment Enrolment { get; set; } = null!;

    }//end of class
}//end of namespace
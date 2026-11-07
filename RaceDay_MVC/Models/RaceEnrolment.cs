
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class RaceEnrolment
    {
        //start of class

        //unique enrolment id
        public int EnrolmentID { get; set; }

        //participant id
        public int ParticipantID { get; set; }

        //selected category id
        public int CategoryID { get; set; }

        //registration date
        public DateTime EnrolmentDate { get; set; }

        //registration status
        public string Status { get; set; } = "";

        //participant name for organiser pages
        public string ParticipantName { get; set; } = "";

        //race category information
        public string CategoryName { get; set; } = "";

        public decimal DistanceKM { get; set; }

        //event information
        public int EventID { get; set; }

        public string EventName { get; set; } = "";

        public DateTime EventDate { get; set; }

        //check whether cancellation is available
        public bool CanCancel =>
            string.Equals(
                Status,
                "Confirmed",
                StringComparison.OrdinalIgnoreCase
            ) && EventDate > DateTime.Now;

        //end of class
    }

    //end of namespace
}


namespace RaceDay_MVC.Models
{
    //start of namespace

    public class ParticipantDashboardViewModel
    {
        //start of class

        //participant's first name
        public string FirstName { get; set; } = "";

        //total confirmed registrations
        public int ConfirmedRegistrations { get; set; }

        //number of upcoming races
        public int UpcomingRaces { get; set; }

        //number of recorded race results
        public int CompletedRaces { get; set; }

        //best finishing position
        public int? BestPosition { get; set; }

        //next upcoming confirmed registrations
        public List<RaceEnrolment> UpcomingEnrolments { get; set; }
            = new List<RaceEnrolment>();

        //most recently captured race results
        public List<RaceResult> RecentResults { get; set; }
            = new List<RaceResult>();

        //end of class
    }

    //end of namespace
}

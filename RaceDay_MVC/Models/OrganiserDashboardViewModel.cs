
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class OrganiserDashboardViewModel
    {
        //start of class

        //total number of events belonging to the organiser
        public int TotalEvents { get; set; }

        //total number of categories across the organiser's events
        public int TotalCategories { get; set; }

        //total number of confirmed race registrations
        public int TotalEnrolments { get; set; }

        //total number of race results captured
        public int TotalResults { get; set; }

        //events that have not yet started
        public List<RaceEvent> UpcomingEvents { get; set; }
            = new List<RaceEvent>();

        //end of class
    }

    //end of namespace
}

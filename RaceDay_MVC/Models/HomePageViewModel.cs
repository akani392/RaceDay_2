
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class HomePageViewModel
    {
        //start of class

        //the next scheduled racing event
        public RaceEvent? NextEvent { get; set; }

        //upcoming racing events
        public List<RaceEvent> UpcomingEvents { get; set; }
            = new List<RaceEvent>();

        //previous racing events
        public List<RaceEvent> PreviousEvents { get; set; }
            = new List<RaceEvent>();

        //indicate if events could not be loaded
        public bool LoadingFailed { get; set; }

        //end of class
    }

    //end of namespace
}

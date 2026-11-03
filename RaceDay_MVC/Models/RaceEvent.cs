namespace RaceDay_MVC.Models
{

    //start of namespace

    public class RaceEvent
    {

        //start of class

        //event id
        public int EventID { get; set; }

        //id for organiser who created the events
        public int OrganiserID { get; set; }

        //event name
        public string EventName { get; set; } = "";

        //date of the event
        public DateTime EventDate { get; set; }

        //event type like running, walking
        public string EventType { get; set; } = "";

        //loction of event
        public string Location { get; set; } = "";

        //eveent description
        public string? Description { get; set; }


        //end of class
    }

    //end of namesoace
}

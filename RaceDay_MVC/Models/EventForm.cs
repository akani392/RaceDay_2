
using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namespace

    public class EventForm
    {
        //start of class

        //event id for editing an existing event
        public int EventID { get; set; }

        //name of the racing event
        [Required(ErrorMessage = "Event name is required")]
        public string EventName { get; set; } = "";

        //date of the event
        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; } =
            DateTime.Today.AddDays(7);

        //location of the event
        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = "";

        //description of the event
        public string Description { get; set; } = "";

        //type of event
        [Required(ErrorMessage = "Event type is required")]
        public string EventType { get; set; } = "";

        //end of class
    }

    //end of namespace
}

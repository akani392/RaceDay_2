using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namespace

    public class EventForm
    {

        //start of class

        //event id for editing existing evets
        public int EventID { get; set; }

        //name of the event
        [Required(ErrorMessage = "Event name is required")]
        public string EventName { get; set; } = "";

        //date of the event
        [Required(ErrorMessage = "Event date is needed")]
        public string Location { get; set; } = "";

        //description of the event
        [Required(ErrorMessage = "Description is needed")]
        public string Description { get; set; } = "";

        //type of event
        [Required(ErrorMessage = "Event type is needed")]
        [RegularExpression("^(Running|Walking|Cycling)$", ErrorMessage = "Select a valid event type")]
        public string EventType { get; set; } = "Running";

        //end of class
    }

    //end of namespace
}

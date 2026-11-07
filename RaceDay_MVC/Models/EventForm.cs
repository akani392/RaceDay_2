
using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namespace

    public class EventForm
    {
        //start of class

        //event identifier used when editing
        public int EventID { get; set; }

        //event name
        [Required(ErrorMessage = "Event name is required")]
        public string EventName { get; set; } = "";

        //event description
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = "";

        //event date
        [Required(ErrorMessage = "Event date is required")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; } = DateTime.Today;

        //event location
        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; } = "";

        //event distance
        [Range(0.1, 1000, ErrorMessage = "Enter a valid distance")]
        public double DistanceKm { get; set; }

        //event type
        [Required(ErrorMessage = "Event type is required")]
        public string EventType { get; set; } = "";

        //end of class
    }

    //end of namespace
}

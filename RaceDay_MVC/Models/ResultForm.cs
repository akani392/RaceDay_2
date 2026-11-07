
using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namespace

    public class ResultForm
    {
        //start of class

        //event that the enrolment belongs to
        public int EventID { get; set; }

        //enrolment selected for result capture
        [Range(1, int.MaxValue,
            ErrorMessage = "Select a valid enrolment")]
        public int EnrolmentID { get; set; }

        //participant finishing time
        [Required(ErrorMessage = "Finish time is required")]
        public TimeSpan? FinishTime { get; set; }

        //participant finishing position
        [Range(1, int.MaxValue,
            ErrorMessage = "Position must be greater than zero")]
        public int Position { get; set; }

        //end of class
    }

    //end of namespace
}

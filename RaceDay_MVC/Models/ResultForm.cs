
using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namespace

    public class ResultForm
    {
        //start of class

        //id of the participant enrolment
        [Range(1, int.MaxValue,ErrorMessage = "Select an enrolment")]
        public int EnrolmentID { get; set; }

        //finishing time entered by organiser
        [Required(ErrorMessage = "Finishing time is required")]
        public TimeSpan? FinishTime { get; set; }

        //finishing position
        [Range(1, int.MaxValue,ErrorMessage = "Position must be greater than zero")]
        public int Position { get; set; }

        //end of class
    }

    //end of namespace
}

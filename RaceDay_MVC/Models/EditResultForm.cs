
using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namespace

    public class EditResultForm
    {
        //start of class

        //existing result being edited
        [Range(1, int.MaxValue)]
        public int ResultID { get; set; }

        //event that owns this result
        public int EventID { get; set; }

        //enrolment linked to this result
        [Range(1, int.MaxValue)]
        public int EnrolmentID { get; set; }

        //updated finishing time
        [Required(ErrorMessage = "Finish time is required")]
        public TimeSpan? FinishTime { get; set; }

        //updated finishing position
        [Range(1, int.MaxValue,
            ErrorMessage = "Position must be greater than zero")]
        public int Position { get; set; }

        //end of class
    }

    //end of namespace
}

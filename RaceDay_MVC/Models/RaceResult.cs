
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class RaceResult
    {
        //start of class

        //unique result identifier
        public int ResultID { get; set; }

        //enrolment linked to the result
        public int EnrolmentID { get; set; }

        //participant's finishing time
        public TimeSpan FinishTime { get; set; }

        //participant's race position
        public int Position { get; set; }

        //end of class
    }

    //end of namespace
}

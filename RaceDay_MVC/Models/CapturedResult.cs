
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class CapturedResult
    {
        //start of class

        //unique result id
        public int ResultID { get; set; }

        //enrolment that owns the result
        public int EnrolmentID { get; set; }

        //organiser who captured the result
        public int CapturedByID { get; set; }

        //recorded finish time
        public TimeSpan FinishTime { get; set; }

        //recorded finishing position
        public int Position { get; set; }

        //date when the result was captured
        public DateTime DateCaptured { get; set; }

        //end of class
    }

    //end of namespace
}

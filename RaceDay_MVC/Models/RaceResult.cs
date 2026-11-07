
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class RaceResult
    {
        //start of class

        //unique result identifier
        public int ResultID { get; set; }

        //enrolment linked to this result
        public int EnrolmentID { get; set; }

        //name of the event
        public string EventName { get; set; } = "";

        //category where the participant competed
        public string CategoryName { get; set; } = "";

        //time taken to finish the race
        public TimeSpan FinishTime { get; set; }

        //finishing position
        public int Position { get; set; }

        //date when the result was captured
        public DateTime DateCaptured { get; set; }

        //end of class
    }

    //end of namespace
}

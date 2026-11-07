
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class ResultRow
    {
        //start of class

        //participant enrolment information
        public RaceEnrolment Enrolment { get; set; }
            = new RaceEnrolment();

        //captured result if available
        public CapturedResult? Result { get; set; }

        //end of class
    }

    public class ResultsPageViewModel
    {
        //start of class

        //selected event id
        public int EventID { get; set; }

        //selected event name
        public string EventName { get; set; } = "";

        //participant enrolments and captured results
        public List<ResultRow> Rows { get; set; }
            = new List<ResultRow>();

        //number of results already captured
        public int TotalCaptured
        {
            //start of property

            get
            {
                //start of get method

                return Rows.Count(r => r.Result != null);

                //end of get method
            }

            //end of property
        }

        //end of class
    }

    //end of namespace
}

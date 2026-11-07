
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class EnrolmentsPageViewModel
    {
        //start of class

        //selected event id
        public int EventID { get; set; }

        //selected event name
        public string EventName { get; set; } = "";

        //all enrolments belonging to the event
        public List<RaceEnrolment> Enrolments { get; set; }
            = new List<RaceEnrolment>();

        //all categories belonging to the event
        public List<RaceCategory> Categories { get; set; }
            = new List<RaceCategory>();

        //calculate the number of confirmed enrolments
        public int TotalConfirmed
        {
            //start of property

            get
            {
                //start of get method

                return Enrolments.Count(e =>
                    e.Status == "Confirmed");

                //end of get method
            }

            //end of property
        }

        //calculate total places across all categories
        public int TotalCapacity
        {
            //start of property

            get
            {
                //start of get method

                return Categories.Sum(c =>
                    c.MaxParticipants);

                //end of get method
            }

            //end of property
        }

        //calculate places still available
        public int AvailablePlaces
        {
            //start of property

            get
            {
                //start of get method

                return Math.Max(
                    0,
                    TotalCapacity - TotalConfirmed
                );

                //end of get method
            }

            //end of property
        }

        //end of class
    }

    //end of namespace
}

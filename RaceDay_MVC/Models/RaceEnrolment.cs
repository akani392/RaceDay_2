namespace RaceDay_MVC.Models
{
    //start of namespace
    public class RaceEnrolment
    {
        //start of class

        //unique id for the enrolment
        public int EnrolmentID { get; set; }

        //id for the particpant
        public int ParticipantID { get; set; } 

        //id of fthe category
        public int CategoryID { get; set; }

        //date when the particpant enrolled
        public DateTime EnrolmentDate { get; set;  }

        //current enrolment status
        public string Status { get; set; } = "";


        //full name of the participant
        public string ParticipantName { get; set; } = "";

        //name of the selected category
        public string CategoryName { get; set; } = "";

        //end of class
    }

    //end of namespace
}

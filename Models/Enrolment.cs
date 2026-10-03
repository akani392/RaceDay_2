using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay_2.Models
{
    //start of namespace
    public class Enrolment
    {
        //start of class
        public int EnrolmentID { get; set; }

        //foreign key to user for participant
        public int ParticipantID { get; set; }
        public User? Participant { get; set; }
        public int CategoryID { get; set;  }
        public Category? Category { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        //confirmed or cancel matches the check constaint
        public string Status { get; set; } = "Confirmed";
        public Result? Result { get; set; }


        //end of class
    }
    //end of namespace
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RaceDay_2.Models
{
    //start of namespace

    public class Result
    {
        //start of class
        [Key]
        public int ResultID { get; set; }

        //puts in a 1 to 1 relationship with enrolment
        public int EnrolmentID { get; set; }
        public Enrolment? Enrolment { get; set; }

        //foreign key to user for the organiser who got the result 
        public int CapturedByID { get; set; }
        public User? CapturedBy { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public DateTime DateCaptured { get; set; } = DateTime.Now;

        //end of class
    }

    //end of namespace
}

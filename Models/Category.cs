using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_2.Models
{
    //start of namespace

    [Table("Category")]
    public class Category
    {
        //start of class

        public int CategoryID { get; set; }

        //foregin key to the event this category belongs to
        public int EventID { get; set; }
        public Event? Event { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKM { get; set; }
        public int MaxParticipants { get; set; }

        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

        //end of class
    }

    //end of namespace
}

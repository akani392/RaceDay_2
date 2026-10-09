using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RaceDay_2.Models
{
    //start of namespace

    public class Event
    {
        //start of class
        [Key]
        public int EventID { get; set; }
           
            //foreifn key to user 
            public int OrganiserID { get; set; }
            public User? Organiser { get; set; }
            public string EventName { get; set; }
            public DateTime EventDate { get; set;  }
            public string EventType { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string? Description { get; set; }
            public List<Category> Categories { get; set; } = new List<Category>();
        
        //end of class
    }

    //end of namespace
}

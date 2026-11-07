
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class RaceCategory
    {
        //start of class

        //unique id of the category
        public int CategoryID { get; set; }

        //event that this category belongs to
        public int EventID { get; set; }

        //name of the racing category
        public string CategoryName { get; set; } = "";

        //distance of the race in kilometres
        public decimal DistanceKM { get; set; }

        //maximum participants allowed
        public int MaxParticipants { get; set; }

        //end of class
    }

    //end of namespace
}

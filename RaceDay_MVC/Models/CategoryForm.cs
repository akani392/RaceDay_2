
using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namespace

    public class CategoryForm
    {
        //start of class

        //id of the event
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        //id of category, 0 means a new category
        public int CategoryID { get; set; }

        //name of racing category
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100)]
        public string CategoryName { get; set; } = "";

        //distance of category
        [Range(typeof(decimal), "0.01", "999.99",
            ErrorMessage = "Distance must be greater than 0")]
        public decimal DistanceKM { get; set; } = 5;

        //maximum number of participants
        [Range(1, 100000,
            ErrorMessage = "Participants must be greater than 0")]
        public int MaxParticipants { get; set; } = 100;

        //end of class
    }

    //end of namespace
}

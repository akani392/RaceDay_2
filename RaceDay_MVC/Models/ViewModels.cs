using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namespace

    public class ProfileForm
    {
        //start of class

        //participant first name
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; } = "";

        //participant last name
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; } = "";

        //participant email address
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [StringLength(100)]
        public string Email { get; set; } = "";

        //end of class
    }

    //end of namespace
}

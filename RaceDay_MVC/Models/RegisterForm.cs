using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{
    //start of namepsace

    public class RegisterForm
    {
        //start of class

        // user first name
        [Required(ErrorMessage = "Your first name is needed")]
        public string FirstName { get; set; } = "";

        //user surname
        [Required(ErrorMessage = "Your last name is need")]
        public string LastName { get; set; } = "";

        //user email
        [Required(ErrorMessage = "Your email is need")]
        [EmailAddress]
        public string Email { get; set; } = "";

        //user password
        [Required(ErrorMessage = "You have to have a password")]
        [MinLength(5, ErrorMessage = "Password has to be minium 5 charcters lomg")]
        public string Password { get; set; } = "";


        //1 for organiser, 2 for participant
        [Range(1, 2)]
        public int RoleID { get; set; } = 2;

        //end of class
    }

    //end of namespace
}

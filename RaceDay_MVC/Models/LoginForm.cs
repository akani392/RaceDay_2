using System.ComponentModel.DataAnnotations;

namespace RaceDay_MVC.Models
{

    //start of namespace

    public class LoginForm
    {

        //start of class

        //user email
        [Required(ErrorMessage = "Email is needed")]
        [EmailAddress(ErrorMessage = "Enter a correct email. Email should end with .com or .edu or .za etc")]
        public string Email { get; set; } = "";

        //user password
        [Required(ErrorMessage = " A password is needed")]
        public string Password { get; set; } = "";

        //end of class

    }

    //end of namespace

}

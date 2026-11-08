
using System.ComponentModel.DataAnnotations;

namespace RaceDay_2.Models
{
    //start of namespace

    public class LoginRequest
    {
        //start of class

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        //end of class
    }

    //end of namespace
}


using System.ComponentModel.DataAnnotations;

namespace RaceDay_2.Models
{
    //start of namespace

    public class RegisterRequest
    {
        //start of class

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = "";

        [Required]
        public int RoleID { get; set; }

        //end of class
    }

    //end of namespace
}

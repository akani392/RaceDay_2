using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RaceDay_2.Models
{
    //start of namespace

    //used as the request body for updating a users own profile
    //matvhes the put in user profile

    public class UpdateProfileRequest
    {
        //start of class
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        //end of class
    }

    //end of naamespace
}

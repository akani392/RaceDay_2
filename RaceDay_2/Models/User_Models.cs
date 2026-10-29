using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_2.Models
{
    //start of namespace
   
    public class User
    {
        //start of class
        [Key]
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public Roles? Role { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime DateRegistered { get; set; } = DateTime.Now;

        //end of class
    }
    //end of namespace
}

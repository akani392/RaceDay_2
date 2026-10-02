using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay_2.Models
{
    //start of namespace
    [Table("Role")]
    public class Roles
    {
        //start of class 
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new List<User>();
        //end of class
    }
    //end of namespace
}

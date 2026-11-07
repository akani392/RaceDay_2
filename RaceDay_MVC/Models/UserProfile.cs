
namespace RaceDay_MVC.Models
{
    //start of namespace

    public class UserProfile
    {
        //start of class

        //unique user identifier
        public int UserID { get; set; }

        //role identifier
        public int RoleID { get; set; }

        //user personal information
        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";

        //date account was created
        public DateTime DateRegistered { get; set; }

        //end of class
    }

    //end of namespace
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RaceDay_2.Models;

namespace RaceDay_2.Controllers
{
    //start of namespace

    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {
        //start of class

        //database context
        private readonly RaceDay_Context _context;

        public Authentication (RaceDay_Context context)
        {
            _context = context;
        }

        //register a new user in the database
        [HttpPost("register")]
        public IActionResult Register ( string firstName, string lastName, string email, string password, int roleId)
        {
            // start of method

            try
            {
                //start of try

                //check that role id matches a real role
                bool role_exists = _context.Roles.Any(r => r.RoleID == roleId);
                if (!role_exists)
                {
                    return BadRequest("RoleID has to match existing role (1 = Organiser, 2 = Participant).");
                }

                //check if email is already there
                bool email_taken = _context.Users.Any(u => u.Email == email);
                if (email_taken)
                {
                    return BadRequest("A user with this email already exits.");
                }

                //hash the password before it is saved
                string password_hash = Password_Helper.hash_password(password);

                //create and save the new user
                var new_user = new User
                {
                    RoleID = roleId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PasswordHash = password_hash
                };

                _context.Users.Add(new_user);
                _context.SaveChanges();

                //Return the created user
                return StatusCode(201, new
                {
                    UserID = new_user.UserID,
                    RoleID = new_user.RoleID,
                    FirstName = new_user.FirstName,
                    LastName = new_user.LastName,
                    Email = new_user.Email
                });

                //end of try
            }

            catch (Exception e)
            {
                //start of catch

                return BadRequest(e.Message);

                //end of catch
            }
        }

        //end of class
    }

    //end of namespace
}

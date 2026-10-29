using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RaceDay_2.Models;
using System.Security.Claims;

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

            //end of method
        }

        //log a user into the system and signs them into a cookie
        //later request know and what roles they have
        [HttpPost("login")]
        public IActionResult Login (string email, string password)
        {
            //start of method

            try
            {
                //start of try

                //finds the user using their email
                //so that the role name is there without a seconf query

                var found_user = _context.Users.Where(u => u.Email == email).Select(u => new
                {
                    u.UserID,
                    u.RoleID,
                    RoleName = u.Role.RoleName,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PasswordHash
                }).SingleOrDefault();

                //check if the user is there
                if (found_user == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                //check if password is correct
                bool password_correct = Password_Helper.verify_password(password, found_user.PasswordHash);

                if (!password_correct)
                {
                    return Unauthorized("Invalid email or password.");
                }

                //get the claims that identify the user in the future
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, found_user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, found_user.Email),
                    new Claim(ClaimTypes.Role, found_user.RoleName)
                };

                var claims_identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claims_principal = new ClaimsPrincipal(claims_identity);

                //sign the user into a cookie
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims_principal).GetAwaiter().GetResult();

                //Login was succesful
                return Ok(new
                {
                    message = "Login Successful 👍😊",
                    UserID = found_user.UserID,
                    FirstName = found_user.FirstName,
                    LastName = found_user.LastName,
                    Email = found_user.Email,
                    RoleName = found_user.RoleName
                });

                //end of try
            }

            catch (Exception e)
            {
                //start of catch
                return BadRequest(e.Message);
                //end of catch
            }
            //end of method
        }

        //sign the user out and clears the authincation cookie
        [HttpPost("logout")]
        public IActionResult Logout ()
        {
            //start of method 

            try
            {
                //start of try
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
                return Ok(new { message = "Logged out successfully 😁✌️" });
                //end of try
            }

            catch (Exception e)
            {
                //start of try

                return BadRequest (e.Message);

                //end of try
            }
        }

        //end of class
    }

    //end of namespace
}

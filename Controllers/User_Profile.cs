using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay_2.Models;
using System.Security.Claims;

namespace RaceDay_2.Controllers
{
    //start of namespace

    [Route("api/[controller]")]
    [ApiController]
    public class User_Profile : ControllerBase
    {
        //start of class

        //database connection
        private readonly RaceDay_Context _context;

        public User_Profile(RaceDay_Context context)
        {
            _context = context;
        }

        //helper method to extract the user id
        private int get_current_user_id()
        {
            //start of method

            //get user id for authintication claim
            var id_claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            //check if UserId is found
            if (id_claim == null || !int.TryParse(id_claim, out int user_id))
            {
                throw new UnauthorizedAccessException("Could not get current user.");
            }

            return user_id;

            //end of method
        }

        //get the current authenciated user profile
        [HttpGet("GetUser")]
        public IActionResult GetUserPprofile()
        {
            //start of metthod
            try
            {
                //start of try

                //get current User id
                int current_user_id = get_current_user_id();

                //find the user
                var user = _context.Users.Find(current_user_id);

                //check if user exists
                if (user == null)
                {
                    return NotFound(new { message = "User profile not found 😢" });
                }

                //return user data
                return Ok(new
                {
                    user.UserID,
                    user.RoleID,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.DateRegistered
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

        //updates the current user profiles
        [HttpPut("UpdateUser")]
        public IActionResult update_user_profile([FromBody] UpdateProfileRequest request)
        {
            //start of method 

            try
            {
                //start of try
                //get the current user id

                int current_user_id = get_current_user_id();

                //find the user to update
                var user = _context.Users.Find(current_user_id);

                if (user == null)
                {
                    return NotFound(new { message = "User profile not found" });
                }

                //check if another user has the same email
                bool email_taken = _context.Users.Any(
                    u => u.Email == request.Email &&
                    u.UserID != current_user_id);

                if (email_taken)
                {
                    return BadRequest(
                        new { message = "That email as already been taken🥲" });
                }

                //update the current user profile
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;

                _context.SaveChanges();

                //rreturn updates user password
                return Ok(new
                {
                    user.UserID,
                    user.RoleID,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.DateRegistered
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

        //end of class
    }

    //end of namespace
}
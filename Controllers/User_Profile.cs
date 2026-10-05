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

        //get the current authenciated user profile
        [HttpGet("GetUser")]

        public IActionResult GetUserProfile()
        {
            //start of method

            try
            {
                //start of try



                //find the user
                var user_id_claim = User.FindFirst(ClaimTypes.NameIdentifier);

                //check if user is there
                if (user_id_claim == null)
                {
                    return Unauthorized(new
                    {
                        message = "User can not be found 😒"
                    });
                }

                //convert user id from string intrger
                if (!int.TryParse(user_id_claim.Value, out var user_id))
                {
                    return BadRequest(new
                    {
                        message = "User name is invalid🤒"
                    });

                    //find the user in the database
                    var user = _context.Users.Find(user_id);

                    //check is user exists
                    if (user == null)
                    {
                        return NotFound(new
                        {
                            message = "User profile can not be found😢"
                        });
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
                }
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

        //updates the current

        //end of class
    }

    //end of namespace
}

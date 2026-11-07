
using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Services;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public abstract class BaseController : Controller
    {
        //start of class

        //talking with raceday 2 api
        protected readonly RaceDayApi Api;

        //constructor for base controller
        protected BaseController(RaceDayApi api)
        {
            //start of constructor

            Api = api;

            //end of constructor
        }

        //check if the user has the correct role
        protected bool HasRole(string role)
        {
            //start of method

            //get the user's role from session
            string? user_role =
                HttpContext.Session.GetString("Role");

            //get the authentication cookie from session
            string? auth_cookie =
                HttpContext.Session.GetString("ApiCookie");

            //check role and authentication cookie
            bool role_matches = string.Equals(
                user_role?.Trim(),
                role,
                StringComparison.OrdinalIgnoreCase
            );

            bool has_cookie =
                !string.IsNullOrWhiteSpace(auth_cookie);

            //display debugging information
            System.Diagnostics.Debug.WriteLine(
                $"User Role: [{user_role}], " +
                $"Required Role: [{role}], " +
                $"Has Cookie: {has_cookie}"
            );

            return role_matches && has_cookie;

            //end of method
        }


        //send users back that do not have permission
        protected IActionResult? RequireRole(string role)
        {
            //start of method

            if (!HasRole(role))
            {
                //start of if

                return RedirectToAction("Login", "Account");

                //end of if
            }

            return null;

            //end of method
        }

        //show success or error message
        protected void Feedback((bool success, string message) result)
        {
            //start of method

            if (result.success)
            {
                //start of if

                TempData["Success"] = result.message;

                //end of if
            }
            else
            {
                //start of else

                TempData["Error"] = result.message;

                //end of else
            }

            //end of method
        }

        //end of class
    }

    //end of namespace
}

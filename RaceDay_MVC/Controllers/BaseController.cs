
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

            string? user_role = HttpContext.Session.GetString("Role");
            string? auth_cookie = HttpContext.Session.GetString("ApiCookie");

            return user_role == role && !string.IsNullOrEmpty(auth_cookie);

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

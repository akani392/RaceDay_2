using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using RaceDay_MVC.Services;

namespace RaceDay_MVC.Controllers
{
    //start of namespace
    public abstract class HomeController : Controller
    {
        //start of class

        //api service talikng with RaceDay 2
        protected readonly RaceDayApi Api;

        //constructor for base controller
        protected BaseController (RaceDayApi api)
        {
            //start of constructor

            Api = api;

            //end of constructor
        }

        //check if logged user has the needed role
        protected bool HasRole(string role)
        {
            //start of method

            string? user_role = HttpContext.Session.GetString("Role");
            string? auth_cookie = HttpContext.Session.GetString("ApiCookie");

            return user_role == role && !string.IsNullOrEmpty(auth_cookie); 

            //enf of method
        }

        //resend the user if they sont not the right password
        protected IActionResult? RequireRole (string role)
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

        //end of class
    }

    //end of namespace
}

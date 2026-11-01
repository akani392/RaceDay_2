using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Services;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public class OrganiserController : BaseController
    {
        //start of class

        //constructor for organiser controller
        public OrganiserController(RaceDayApi api) : base(api) { }

        //show the organiser dashboard
        [HttpGet]
        public IActionResult Index()
        {
            //start of method

            //check if the user is an organiser
            var check = RequireRole("Organiser");

            if (check != null)
            {
                //start of if

                return check;

                //end of if
            }

            //show the organiser info
            return View();

            //end of method
        }

        //end of class
    }

    //end of namespace
}

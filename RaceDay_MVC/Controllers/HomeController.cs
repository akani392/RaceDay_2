
using Microsoft.AspNetCore.Mvc;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public class HomeController : Controller
    {
        //start of class

        //display the main RaceDay home page
        [HttpGet]
        public IActionResult Index()
        {
            //start of method

            return View();

            //end of method
        }

        //end of class
    }

    //end of namespace
}

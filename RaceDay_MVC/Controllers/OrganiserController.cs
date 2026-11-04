using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Services;
using RaceDay_MVC.Models;

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

        //show all events by thecorganiser
        [HttpGet]
        public async Task<IActionResult> Events()
        {
            //start of method

            //check if the user is a orgainser
            var check = RequireRole("Organizer");

            if (check != null)
            {
                //start of if

                return check;


                //end of id
            }

            try
            {
                //start of try
                //get all the events
                var all_events = await Api.Get<List<RaceEvent>>(HttpContext, "api/Events");

                //get the logged on orgainser users id
                int? organiser_id = HttpContext.Session.GetInt32("UserID");

                //only display events owned by this orgainser
                var my_events = (all_events ?? new List<RaceEvent>()).Where(e => e.OrganiserID == organiser_id).OrderBy(e => e.EventDate).ToList();
                return View(my_events);

            }

            catch (Exception e)
            {
                //start of catch 

                TempData["Error"] = " unable to connect to the RaceDay API";

                return View(new List<RaceEvent>());

                //end of catch
            }

            //end of method
        }

        //end of class
    }

    //end of namespace
}

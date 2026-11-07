
using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public class ParticipantController : BaseController
    {
        //start of class

        //constructor for participant controller
        public ParticipantController(RaceDayApi api) : base(api)
        {
            //start of constructor

            //end of constructor
        }

        //display available racing events for participants
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //start of method

            //check participant permission
            var check = RequireRole("Participant");

            if (check != null)
            {
                //start of if

                return check;

                //end of if
            }

            try
            {
                //start of try

                //get all racing events from RaceDay API
                var events = await Api.Get<List<RaceEvent>>(
                    HttpContext,
                    "api/Events"
                );

                //check if events were returned
                if (events == null)
                {
                    //start of if

                    TempData["Error"] =
                        "Unable to load racing events.";

                    return View(new List<RaceEvent>());

                    //end of if
                }

                //show upcoming events first
                var available_events = events
                    .Where(e => e.EventDate >= DateTime.Today)
                    .OrderBy(e => e.EventDate)
                    .ToList();

                //display the event list
                return View(available_events);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return View(new List<RaceEvent>());

                //end of catch
            }

            //end of method
        }

        //end of class
    }

    //end of namespace
}

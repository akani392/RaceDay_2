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


        //show the create eevent page
        [HttpGet]
        public IActionResult CreateEvent()
        {
            //start of method
            //check organiser permission

            var check = RequireRole("Organiser");

            if (check != null)
            {
                ///start of if

                return check;

                ///end of if
            }

            //show emppty event form
            return View("EventForm", new EventForm());

            //end of method
        }

        //show the edit event page
        [HttpGet]
        public async Task <IActionResult> EditEvent(int id)
        {
            //start of method

            //check organiser permission
            var check = RequireRole("Organiser");

            if (check !=null)
            {
                //start of if

                return check;

                //end of if
            }

            try
            {
                //start of try

                //get event from raceday 2 api
                var found_event = await Api.Get<RaceEvent>(HttpContext, $"api/Events/{id}");

                //get current organiser id
                int? organiser_id = HttpContext.Session.GetInt32("UserID");

                //check event exists
                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //check evet ownership
                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //emd of if
                }

                //populate form eith existing event information
                var form = new EventForm
                {
                    EventID = found_event.EventID,
                    EventName = found_event.EventName,
                    EventType = found_event.EventType,
                    Location = found_event.Location,
                    Description = found_event.Description ?? ""
                };

                return View("EventForm", form);

                //end of try
            }

            catch (Exception e)
            {
                //start of catch 

                TempData["Error"] = "Unable to connect to the RaceDay API";

                return RedirectToAction(nameof(Events));

                //end of catch
            }

            //end of method
        }

        //end of class
    }

    //end of namespace
}

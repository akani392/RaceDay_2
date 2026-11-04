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

        //create or update an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEvent(EventForm form)
        {
            //start of method

            //check organiser permission
            var check = RequireRole("Organiser");

            if (check != null)
            {
                //start of if

                return check;

                //end of if
            }

            //validate user input
            if (!ModelState.IsValid)
            {

                //start of if

                return View("EventForm", form);

                
                //end of if
            }

            try
            {
                //start of try
                //check ownweship before editing an existing event
                if (form.EventID > 0)
                {
                    //start of if

                    var existing_event = await Api.Get<RaceEvent>(HttpContext, $"api/Events/{form.EventID}");
                    int? organiser_id = HttpContext.Session.GetInt32("UserID");

                    if (existing_event == null)
                    {
                        //start of if

                        return NotFound();

                        //end of if
                    }

                    if (existing_event.OrganiserID != organiser_id)
                    {
                        //start of if

                        return Forbid();

                        //end of if
                    }

                    //end of if
                }


                //get the right api route
                string route = "api/Events";

                //determine create or update operation
                HttpMethod method = HttpMethod.Post;

                if (form.EventID > 0)
                {
                    //start of if

                    route += $"/{form.EventID}";
                    method = HttpMethod.Put;

                    //end of if
                }

                //add event information
                route += "?" + RaceDayApi.Query(
                    ("eventName", form.EventName),
                    ("location", form.Location),
                    ("description", form.Description),
                    ("eventType", form.EventType));

                //sebd rrequest to RaceDay 2 API
                var result = await Api.Write(HttpContext, method, route);

                //display success or error message
                Feedback(result);

                if (result.success)
                {
                    //start of if

                    return RedirectToAction(nameof(Events));


                    //end of if
                }

                return View("EventForm", form);



                //end of try
            }

            catch
            {
                //start of catch

                ModelState.AddModelError("", "Unable to connect to the RaceDay API");

                return View("EventForm", form);

                //end of catch
            }

            //end of method
        }


        //delete an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            //start of method 

            //check orgainser permission
            var check = RequireRole("Organiser");

            if (check != null)
            {
                //start of if

                return check;

                //of if
            }

            try
            {
                //start of try

                //get event details
                var found_event = await Api.Get<RaceEvent>(HttpContext, $"api/Events/{id}");

                int? organiser_id = HttpContext.Session.GetInt32("UserID");

                //check event exists
                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //send delete requested to api
                var result = await Api.Write(HttpContext, HttpMethod.Delete, $"api/Events/{id}");
                Feedback(result);

                return RedirectToAction(nameof(Events));


                //end of try
            }

            catch (Exception e)
            {
                //start of catch

                TempData["Error"] = "unable to connect to the raceday api";

                return RedirectToAction(nameof(Events));

                //end of catch
            }

            //end of method
        }

        //end of class
    }

    //end of namespace
}

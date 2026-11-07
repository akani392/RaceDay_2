
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


        //display details and categories ffor a selected event
        [HttpGet]
        public async Task <IActionResult> Details(int id)
        {
            //start of method

            //check participants permissions
            var check = RequireRole("Paerticipant");
            if(check != null)
            {
                //start of if

                return check;

                //end of if
            }

            //check event id
            if (id <= 0)
            {
                //start of if

                return RedirectToAction("Index");

                //end of if
            }

            try
            {
                //start of try

                //get the selected event from the apoi
                var found_event = await Api.Get<RaceEvent>(HttpContext, $"api/Events/{id}");

                //check if event exists
                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //get the event categories
                var categories =  await Api.Get<List<RaceCategory>>(HttpContext,$"api/events/{id}/categories");

                //check if categories loaded
                if (categories == null)
                {
                    //start of if

                    TempData["Error"] = "unable to load event categories";

                    return RedirectToAction("Index");

                    //end of if
                }

                //add categorikes to the event
                found_event.Categories = categories;

                //get the participants existing enrolments
                var enrolments = await Api.Get<List<RaceEnrolment>>(HttpContext, "api/enrolments/me");

                //check if existing enrolments loaded
                if(enrolments == null)
                {
                    //start of if

                    TempData["Error"] = "Unable to check your exiting registrations";
                    return RedirectToAction("Index");

                    //end of if
                }

                //store confirmed category id for the razor view
                ViewBag.EnrolledCategoryIDs = enrolments.Where(e => e.Status == "Confirmed").Select(e => e.CategoryID).ToList();

                //display the vent details
                return View(found_event);

                //end of try
            }

            catch (Exception e)
            {
                //start of catch

                TempData["Error"] = "Unable to connect to the RaceDay API";
                return RedirectToAction("Index");

                //end of catch
            }

            //end of method
        }


        //register a participant for a race category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(
            int categoryId,
            int eventId)
        {
            //start of method

            //check participant permissions
            var check = RequireRole("Participant");

            if (check != null)
            {
                //start of if

                return check;

                //end of if
            }

            //validate event and category ids
            if (eventId <= 0 || categoryId <= 0)
            {
                //start of if

                return BadRequest();

                //end of if
            }

            try
            {
                //start of try

                //get selected event
                var found_event = await Api.Get<RaceEvent>(
                    HttpContext,
                    $"api/Events/{eventId}"
                );

                //check that event exists and is upcoming
                if (found_event == null ||
                    found_event.EventDate < DateTime.Now)
                {
                    //start of if

                    TempData["Error"] =
                        "This event is unavailable for registration.";

                    return RedirectToAction("Index");

                    //end of if
                }

                //get categories belonging to the event
                var categories = await Api.Get<List<RaceCategory>>(
                    HttpContext,
                    $"api/events/{eventId}/categories"
                );

                //confirm selected category belongs to the event
                bool valid_category = categories?.Any(c =>
                    c.CategoryID == categoryId) == true;

                if (!valid_category)
                {
                    //start of if

                    TempData["Error"] =
                        "The selected category is invalid.";

                    return RedirectToAction(
                        "Details",
                        new { id = eventId }
                    );

                    //end of if
                }

                //prepare the enrolment route
                string route = "api/enrolments?"
                    + RaceDayApi.Query(
                        ("categoryId", categoryId.ToString())
                    );

                //send enrolment request to the API
                var result = await Api.Write(
                    HttpContext,
                    HttpMethod.Post,
                    route
                );

                //display success or error message
                Feedback(result);

                //return to the event details
                return RedirectToAction(
                    "Details",
                    new { id = eventId }
                );

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction(
                    "Details",
                    new { id = eventId }
                );

                //end of catch
            }

            //end of method
        }


        //end of class
    }

    //end of namespace
}

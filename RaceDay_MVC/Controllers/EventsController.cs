
using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public class EventsController : BaseController
    {
        //start of class

        //constructor for events controller
        public EventsController(RaceDayApi api) : base(api)
        {
            //start of constructor

            //end of constructor
        }

   


        //display details of a selected event
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            //start of method

            //check event id
            if (id <= 0)
            {
                //start of if

                return NotFound();

                //end of if
            }

            try
            {
                //start of try

                //get the selected event and categories
                var found_event = await Api.Get<RaceEvent>(
                    HttpContext,
                    $"api/Events/{id}"
                );

                //check if event exists
                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //display event details
                return View(found_event);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to retrieve event details.";

                return RedirectToAction("Index");

                //end of catch
            }
        }


        //display and filter available racing events
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? type,
            string? period,
            string? sort)
        {
            //start of method

            //store selected filters for the razor view
            ViewBag.Search = search;
            ViewBag.Type = type;
            ViewBag.Period = period;
            ViewBag.Sort = sort;

            try
            {
                //start of try

                //load events from the public API
                var events = await Api.Get<List<RaceEvent>>(
                    HttpContext,
                    "api/Events"
                );

                //check if the API returned events
                if (events == null)
                {
                    //start of if

                    TempData["Error"] =
                        "Unable to load racing events.";

                    return View(new List<RaceEvent>());

                    //end of if
                }

                //search by event name or location
                if (!string.IsNullOrWhiteSpace(search))
                {
                    //start of if

                    string keyword = search.Trim();

                    events = events.Where(e =>
                        e.EventName.Contains(
                            keyword,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        e.Location.Contains(
                            keyword,
                            StringComparison.OrdinalIgnoreCase)
                    ).ToList();

                    //end of if
                }

                //filter by race type
                if (!string.IsNullOrWhiteSpace(type))
                {
                    //start of if

                    events = events.Where(e =>
                        string.Equals(
                            e.EventType,
                            type,
                            StringComparison.OrdinalIgnoreCase)
                    ).ToList();

                    //end of if
                }

                //filter by upcoming or previous events
                if (period == "upcoming")
                {
                    //start of if

                    events = events.Where(e =>
                        e.EventDate >= DateTime.Now
                    ).ToList();

                    //end of if
                }
                else if (period == "previous")
                {
                    //start of else if

                    events = events.Where(e =>
                        e.EventDate < DateTime.Now
                    ).ToList();

                    //end of else if
                }

                //sort events by selected preference
                if (sort == "latest")
                {
                    //start of if

                    events = events
                        .OrderByDescending(e => e.EventDate)
                        .ToList();

                    //end of if
                }
                else if (sort == "name")
                {
                    //start of else if

                    events = events
                        .OrderBy(e => e.EventName)
                        .ToList();

                    //end of else if
                }
                else
                {
                    //start of else

                    events = events
                        .OrderBy(e => e.EventDate)
                        .ToList();

                    //end of else
                }

                //display filtered events
                return View(events);

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
        }


        //end of class
    }

    //end of namespace
}

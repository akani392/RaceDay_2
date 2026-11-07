
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

        //display available racing events
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? type)
        {
            //start of method

            //check if the user is logged in
            if (!HasRole("Participant") &&
                !HasRole("Organiser"))
            {
                //start of if

                return RedirectToAction("Login", "Account");

                //end of if
            }

            //get all events from the API
            var events = await Api.Get<List<RaceEvent>>(
                HttpContext,
                "api/Events"
            );

            //check if the API returned events
            if (events == null)
            {
                //start of if

                TempData["Error"] =
                    "Unable to load events from the API.";

                return View(new List<RaceEvent>());

                //end of if
            }

            //search by event name or location
            if (!string.IsNullOrWhiteSpace(search))
            {
                //start of if

                events = events.Where(e =>
                    e.EventName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    e.Location.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase)
                ).ToList();

                //end of if
            }

            //filter by event type
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

            //display upcoming events first
            events = events
                .OrderBy(e => e.EventDate)
                .ToList();

            //display events page
            return View(events);

            //end of method
        }

        //display details of the selected event
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            //start of method

            //check if the user is logged in
            if (!HasRole("Participant") &&
                !HasRole("Organiser"))
            {
                //start of if

                return RedirectToAction("Login", "Account");

                //end of if
            }

            //get selected event from API
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

            //get categories for the event
            var categories = await Api.Get<List<RaceCategory>>(
                HttpContext,
                $"api/events/{id}/categories"
            );


            //display category information for debugging
            System.Diagnostics.Debug.WriteLine(
                $"Selected Event ID: {id}"
            );

            System.Diagnostics.Debug.WriteLine(
                $"Categories returned: {categories?.Count ?? 0}"
            );

            //display each category
            foreach (var category in categories ?? new List<RaceCategory>())
            {
                //start of foreach

                System.Diagnostics.Debug.WriteLine(
                    $"Category ID: {category.CategoryID}, " +
                    $"Event ID: {category.EventID}, " +
                    $"Category Name: {category.CategoryName}"
                );

                found_event.Categories = categories;

                //end of foreach
            }


            //check if categories could be loaded
            if (categories == null)
            {
                //start of if

                TempData["Error"] =
                    "Unable to load event categories.";

                return RedirectToAction("Index");

                //end of if
            }

            //save categories in the event model
            found_event.Categories = categories;

            //display event details page
            return View(found_event);

            //end of method
        }

        //end of class
    }

    //end of namespace
}

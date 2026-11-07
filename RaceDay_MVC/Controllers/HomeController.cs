
using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
using System.Text.Json;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public class HomeController : Controller
    {
        //start of class

        //api service for getting racing events
        private readonly RaceDayApi _api;

        //constructor for home controller
        public HomeController(RaceDayApi api)
        {
            //start of constructor

            _api = api;

            //end of constructor
        }

        //display the public homepage
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //start of method

            //create the homepage model
            var model = new HomePageViewModel();

            try
            {
                //start of try

                //get events without requiring login
                var events = await _api.Get<List<RaceEvent>>(
                    HttpContext,
                    "api/Events"
                );

                //check if events were loaded
                if (events == null)
                {
                    //start of if

                    model.LoadingFailed = true;

                    return View(model);

                    //end of if
                }

                //find upcoming events
                model.UpcomingEvents = events
                    .Where(e => e.EventDate >= DateTime.Now)
                    .OrderBy(e => e.EventDate)
                    .Take(6)
                    .ToList();

                //get the next scheduled event
                model.NextEvent = model.UpcomingEvents
                    .FirstOrDefault();

                //find previous events
                model.PreviousEvents = events
                    .Where(e => e.EventDate < DateTime.Now)
                    .OrderByDescending(e => e.EventDate)
                    .Take(3)
                    .ToList();

                //display homepage
                return View(model);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                model.LoadingFailed = true;

                return View(model);

                //end of catch
            }
            catch (JsonException)
            {
                //start of catch

                model.LoadingFailed = true;

                return View(model);

                //end of catch
            }

            //end of method
        }


        //display an error page when an unexpected problem occurs
        [HttpGet]
        public IActionResult Error()
        {
            //start of method

            return View();

            //end of method
        }


        //end of class
    }

    //end of namespace
}

using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
using System.Net;
using System.Net.Http.Json;

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

            //check participant permissions
            var check = RequireRole("Participant");
            if (check != null)
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


        //display the participant's race enrolments
        [HttpGet]
        public async Task<IActionResult> Enrolments()
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

                //get participant enrolments from the API
                using var response = await Api.Send(
                    HttpContext,
                    HttpMethod.Get,
                    "api/enrolments/me"
                );

                //check if login session is no longer valid
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //start of if

                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");

                    //end of if
                }

                //check API permissions
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check if the request failed
                if (!response.IsSuccessStatusCode)
                {
                    //start of if

                    TempData["Error"] =
                        "Unable to load your race enrolments.";

                    return View(new List<RaceEnrolment>());

                    //end of if
                }

                //convert API response into a list
                var enrolments = await response.Content
                    .ReadFromJsonAsync<List<RaceEnrolment>>();

                //send enrolments to the Razor view
                return View(
                    enrolments ?? new List<RaceEnrolment>()
                );

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return View(new List<RaceEnrolment>());

                //end of catch
            }

            //end of method
        }


        //cancel the participant's selected enrolment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
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

            //check enrolment id
            if (id <= 0)
            {
                //start of if

                TempData["Error"] = "Invalid enrolment selected.";

                return RedirectToAction(nameof(Enrolments));

                //end of if
            }

            try
            {
                //start of try

                //request enrolment cancellation through the API
                using var response = await Api.Send(
                    HttpContext,
                    HttpMethod.Delete,
                    $"api/enrolments/{id}"
                );

                //check if cancellation succeeded
                if (response.IsSuccessStatusCode)
                {
                    //start of if

                    TempData["Success"] =
                        "Your race registration has been cancelled.";

                    //end of if
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //start of else if

                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");

                    //end of else if
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of else if

                    TempData["Error"] =
                        "You cannot cancel this registration.";

                    //end of else if
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    //start of else if

                    TempData["Error"] =
                        "The selected enrolment could not be found.";

                    //end of else if
                }
                else
                {
                    //start of else

                    TempData["Error"] =
                        "Cancellation failed. The event may have started, " +
                        "or the registration may already be cancelled.";

                    //end of else
                }

                //return to participant enrolments
                return RedirectToAction(nameof(Enrolments));

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction(nameof(Enrolments));

                //end of catch
            }

            //end of method
        }


        //display the participant's personal race results
        [HttpGet]
        public async Task<IActionResult> Results()
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

                //request results belonging to the logged in participant
                using var response = await Api.Send(
                    HttpContext,
                    HttpMethod.Get,
                    "api/users/me/results"
                );

                //check whether the api authentication expired
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //start of if

                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");

                    //end of if
                }

                //check participant permissions
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check if the api request failed
                if (!response.IsSuccessStatusCode)
                {
                    //start of if

                    TempData["Error"] =
                        "Unable to retrieve your race results.";

                    return View(new List<RaceResult>());

                    //end of if
                }

                //convert api results into race result objects
                var results = await response.Content
                    .ReadFromJsonAsync<List<RaceResult>>();

                //order results by date captured
                var ordered_results = (results ?? new List<RaceResult>())
                    .OrderByDescending(r => r.DateCaptured)
                    .ToList();

                //display race results page
                return View(ordered_results);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return View(new List<RaceResult>());

                //end of catch
            }

            //end of method
        }


        //display the participant's personal profile
        [HttpGet]
        public async Task<IActionResult> Profile()
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

            try
            {
                //start of try

                //request the current user's profile
                using var response = await Api.Send(
                    HttpContext,
                    HttpMethod.Get,
                    "api/User_Profile/GetUser"
                );

                //check if login has expired
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //start of if

                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");

                    //end of if
                }

                //check whether access is denied
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check whether the profile loaded
                if (!response.IsSuccessStatusCode)
                {
                    //start of if

                    TempData["Error"] =
                        "Unable to retrieve your profile.";

                    return RedirectToAction("Index");

                    //end of if
                }

                //convert api response to user profile
                var user = await response.Content
                    .ReadFromJsonAsync<UserProfile>();

                //check if profile was returned
                if (user == null)
                {
                    //start of if

                    TempData["Error"] = "User profile not found.";

                    return RedirectToAction("Index");

                    //end of if
                }

                //populate the edit form
                var form = new ProfileForm
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                };

                //display the profile page
                return View(form);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Index");

                //end of catch
            }

            //end of method
        }


        //update the participant's personal profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileForm form)
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

            //check that form inputs are valid
            if (!ModelState.IsValid)
            {
                //start of if

                return View(form);

                //end of if
            }

            //remove unnecessary spaces
            form.FirstName = form.FirstName.Trim();
            form.LastName = form.LastName.Trim();
            form.Email = form.Email.Trim();

            //check if trimmed names are empty
            if (string.IsNullOrWhiteSpace(form.FirstName) ||
                string.IsNullOrWhiteSpace(form.LastName))
            {
                //start of if

                ModelState.AddModelError(
                    "",
                    "First name and last name cannot be empty."
                );

                return View(form);

                //end of if
            }

            try
            {
                //start of try

                //send updated profile to the API as JSON
                using var response = await Api.Send(
                    HttpContext,
                    HttpMethod.Put,
                    "api/User_Profile/UpdateUser",
                    form
                );

                //check for expired login
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //start of if

                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");

                    //end of if
                }

                //check whether access is denied
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check whether update succeeded
                if (response.IsSuccessStatusCode)
                {
                    //start of if

                    //update displayed name in current session
                    HttpContext.Session.SetString(
                        "Name",
                        form.FirstName
                    );

                    //show confirmation message
                    TempData["Success"] =
                        "Your profile has been updated successfully.";

                    //reload the updated profile
                    return RedirectToAction(nameof(Profile));

                    //end of if
                }

                //check whether the email is already used
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    //start of if

                    ModelState.AddModelError(
                        "",
                        "Unable to save these details. The email address " +
                        "may already be registered, or an input may be invalid."
                    );

                    return View(form);

                    //end of if
                }

                //handle unexpected API responses
                ModelState.AddModelError(
                    "",
                    "Unable to update your profile. Please try again."
                );

                return View(form);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                ModelState.AddModelError(
                    "",
                    "Unable to connect to the RaceDay API."
                );

                return View(form);

                //end of catch
            }

            //end of method
        }


        //end of class
    }

    //end of namespace
}

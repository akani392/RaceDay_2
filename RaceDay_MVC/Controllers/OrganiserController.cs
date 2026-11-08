using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
using System.Net;
using System.Net.Http.Json;
using System.Globalization;

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
        public async Task<IActionResult> Index()
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

            //create the dashboard model
            var model = new OrganiserDashboardViewModel();

            //get the logged in organiser id
            int? organiser_id =
                HttpContext.Session.GetInt32("UserID");

            //check organiser id
            if (organiser_id == null)
            {
                //start of if

                return RedirectToAction("Login", "Account");

                //end of if
            }

            try
            {
                //start of try

                //get all racing events from the API
                var all_events = await Api.Get<List<RaceEvent>>(
                    HttpContext,
                    "api/Events"
                );

                //check if events loaded
                if (all_events == null)
                {
                    //start of if

                    throw new HttpRequestException(
                        "Unable to load events."
                    );

                    //end of if
                }

                //only get events owned by this organiser
                var my_events = all_events
                    .Where(e => e.OrganiserID == organiser_id.Value)
                    .ToList();

                //count the organiser's events
                model.TotalEvents = my_events.Count;

                //get upcoming events
                model.UpcomingEvents = my_events
                    .Where(e => e.EventDate >= DateTime.Now)
                    .OrderBy(e => e.EventDate)
                    .Take(5)
                    .ToList();

                //get categories and enrolments for each event
                foreach (var race_event in my_events)
                {
                    //start of foreach

                    //get categories belonging to this event
                    var categories = await Api.Get<List<RaceCategory>>(
                        HttpContext,
                        $"api/events/{race_event.EventID}/categories"
                    );

                    //check categories loaded
                    if (categories == null)
                    {
                        //start of if

                        throw new HttpRequestException(
                            "Unable to load categories."
                        );

                        //end of if
                    }

                    //add event categories to total
                    model.TotalCategories += categories.Count;

                    //get the event's enrolments
                    using var enrolment_response = await Api.Send(
                        HttpContext,
                        HttpMethod.Get,
                        $"api/events/{race_event.EventID}/enrolments"
                    );

                    //check whether the request succeeded
                    enrolment_response.EnsureSuccessStatusCode();

                    //convert response to enrolments
                    var enrolments = await enrolment_response.Content
                        .ReadFromJsonAsync<List<RaceEnrolment>>()
                        ?? new List<RaceEnrolment>();

                    //only count confirmed registrations
                    var confirmed_enrolments = enrolments
                        .Where(e => string.Equals(
                            e.Status,
                            "Confirmed",
                            StringComparison.OrdinalIgnoreCase
                        ))
                        .ToList();

                    //add confirmed registrations to total
                    model.TotalEnrolments += confirmed_enrolments.Count;

                    //check captured results for each enrolment
                    foreach (var enrolment in confirmed_enrolments)
                    {
                        //start of foreach

                        //request the participant's result
                        using var result_response = await Api.Send(
                            HttpContext,
                            HttpMethod.Get,
                            $"api/enrolments/{enrolment.EnrolmentID}/result"
                        );

                        //count captured result
                        if (result_response.IsSuccessStatusCode)
                        {
                            //start of if

                            model.TotalResults++;

                            //end of if
                        }
                        else if (result_response.StatusCode != HttpStatusCode.NotFound)
                        {
                            //start of else if

                            result_response.EnsureSuccessStatusCode();

                            //end of else if
                        }

                        //end of foreach
                    }

                    //end of foreach
                }

                //display the dashboard
                return View(model);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to load dashboard statistics from the RaceDay API.";

                //do not display incomplete totals as accurate statistics
                return RedirectToAction("Events");

                //end of catch
            }
            catch (System.Text.Json.JsonException)
            {
                //start of catch

                TempData["Error"] =
                    "The RaceDay API returned invalid dashboard data.";

                return RedirectToAction("Events");

                //end of catch
            }

            //end of method
        }



        //show all events by the organiser
        [HttpGet]
        public async Task<IActionResult> Events()
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

                //populate form with existing event information
                var form = new EventForm
                {
                    EventID = found_event.EventID,
                    EventName = found_event.EventName,
                    EventDate = found_event.EventDate,
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


                //add event information to the api request
                route += "?" + RaceDayApi.Query(
                    ("eventName", form.EventName),
                    ("eventDate", form.EventDate.ToString("yyyy-MM-dd")),
                    ("location", form.Location),
                    ("description", form.Description),
                    ("eventType", form.EventType)
                );


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


        //show categories for a selected event
        [HttpGet]
        public async Task<IActionResult> Categories(int id)
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

            try
            {
                //start of try

                //get the selected event
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

                //get current organiser id
                int? organiser_id =
                    HttpContext.Session.GetInt32("UserID");

                //check if organiser owns the event
                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //get categories belonging to the event
                var categories = await Api.Get<List<RaceCategory>>(
                    HttpContext,
                    $"api/events/{id}/categories"
                );

                //save the event details for the view
                ViewBag.EventID = id;
                ViewBag.EventName = found_event.EventName;

                //display categories
                return View(categories ?? new List<RaceCategory>());

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Events");

                //end of catch
            }

            //end of method
        }


        //show the create category form
        [HttpGet]
        public async Task<IActionResult> CreateCategory(int eventId)
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

            //get the selected event
            var found_event = await Api.Get<RaceEvent>(
                HttpContext,
                $"api/Events/{eventId}"
            );

            //check if event exists
            if (found_event == null)
            {
                //start of if

                return NotFound();

                //end of if
            }

            //check event ownership
            int? organiser_id =
                HttpContext.Session.GetInt32("UserID");

            if (found_event.OrganiserID != organiser_id)
            {
                //start of if

                return Forbid();

                //end of if
            }

            //create category form
            var form = new CategoryForm
            {
                EventID = eventId
            };

            return View("CategoryForm", form);

            //end of method
        }


        //show the edit category form
        [HttpGet]
        public async Task<IActionResult> EditCategory(int eventId, int id)
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

            //get the selected event
            var found_event = await Api.Get<RaceEvent>(
                HttpContext,
                $"api/Events/{eventId}"
            );

            if (found_event == null)
            {
                //start of if

                return NotFound();

                //end of if
            }

            //check event ownership
            int? organiser_id =
                HttpContext.Session.GetInt32("UserID");

            if (found_event.OrganiserID != organiser_id)
            {
                //start of if

                return Forbid();

                //end of if
            }

            //get categories for this event
            var categories = await Api.Get<List<RaceCategory>>(
                HttpContext,
                $"api/events/{eventId}/categories"
            );

            //find the selected category
            var category = categories?
                .FirstOrDefault(c => c.CategoryID == id);

            if (category == null)
            {
                //start of if

                return NotFound();

                //end of if
            }

            //populate the category form
            var form = new CategoryForm
            {
                EventID = category.EventID,
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                DistanceKM = category.DistanceKM,
                MaxParticipants = category.MaxParticipants
            };

            return View("CategoryForm", form);

            //end of method
        }


        //create or update a racing category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategory(CategoryForm form)
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

            //validate category information
            if (!ModelState.IsValid)
            {
                //start of if

                return View("CategoryForm", form);

                //end of if
            }

            try
            {
                //start of try

                //get the event that owns the category
                var found_event = await Api.Get<RaceEvent>(
                    HttpContext,
                    $"api/Events/{form.EventID}"
                );

                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //check organiser ownership
                int? organiser_id =
                    HttpContext.Session.GetInt32("UserID");

                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check that the category belongs to this event
                if (form.CategoryID > 0)
                {
                    //start of if

                    var categories = await Api.Get<List<RaceCategory>>(HttpContext,$"api/events/{form.EventID}/categories");

                    bool category_exists = categories?.Any(c => c.CategoryID == form.CategoryID) == true;

                    if (!category_exists)
                    {
                        //start of if

                        return NotFound();

                        //end of if
                    }

                    //end of if
                }

                //choose create or update route
                string route = form.CategoryID > 0? $"api/Categories/{form.CategoryID}": $"api/events/{form.EventID}/categories";

                //add the category parameters
                route += "?" + RaceDayApi.Query(
                    ("categoryName", form.CategoryName),
                    ("distanceKm", form.DistanceKM.ToString(
                        CultureInfo.InvariantCulture)),
                    ("maxParticipants", form.MaxParticipants.ToString())
                );

                //choose the correct HTTP method
                HttpMethod method = form.CategoryID > 0? HttpMethod.Put: HttpMethod.Post;

                //send request to the API
                var result = await Api.Write(HttpContext,method,route);

                //display feedback message
                Feedback(result);

                if (result.success)
                {
                    //start of if

                    return RedirectToAction("Categories",new { id = form.EventID });

                    //end of if
                }

                return View("CategoryForm", form);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                ModelState.AddModelError("","Unable to connect to the RaceDay API.");

                return View("CategoryForm", form);

                //end of catch
            }

            //end of method
        }


        //delete a racing category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
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

            try
            {
                //start of try

                //get event details
                var found_event = await Api.Get<RaceEvent>(HttpContext,$"api/Events/{eventId}"
                );

                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //check that organiser owns the event
                int? organiser_id =HttpContext.Session.GetInt32("UserID");

                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check that the category belongs to this event
                var categories = await Api.Get<List<RaceCategory>>(HttpContext,$"api/events/{eventId}/categories");

                if (categories?.Any(c => c.CategoryID == id) != true)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //delete category using the API
                var result = await Api.Write(HttpContext,HttpMethod.Delete, $"api/Categories/{id}");

                Feedback(result);

                return RedirectToAction("Categories",new { id = eventId });

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] ="Unable to connect to the RaceDay API.";

                return RedirectToAction("Categories",new { id = eventId });

                //end of catch
            }

            //end of method
        }

        //show enrolments belonging to an organiser event
        [HttpGet]
        public async Task<IActionResult> Enrolments(
            int id,
            int? categoryId,
            string? status)
        {
            //start of method

            //check oeganiser permissions
            var check = RequireRole("Organiser");

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

                return RedirectToAction("Events");

                //end of if
            }


            try
            {
                //start of try

                //get the selected event
                var found_event = await Api.Get<RaceEvent>(HttpContext,$"api/Events/{id}");

                //check if events exists
                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //get the logged in organiser
                int? organiser_id = HttpContext.Session.GetInt32("UserID");

                //check event ownership
                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if


                    return Forbid();

                    //end of if
                }

                //request enrolments from the api
                using var response = await Api.Send(HttpContext, HttpMethod.Get, $"api/events/{id}/enrolments");

                //check if api rejected access
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check if session expired
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //start of if

                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");

                    //end of if
                }

                //check if enrolment request failed
                if (!response.IsSuccessStatusCode)
                {
                    //start of if

                    TempData["Error"] = "Unable to load event enrolments";

                    return RedirectToAction("Events");

                    //end of if
                }

                //convert the api response to enrolment objects
                var enrolments = await response.Content.ReadFromJsonAsync<List<RaceEnrolment>>();

                //get all categories for the selected event
                var categories = await Api.Get<List<RaceCategory>>(HttpContext, $"api/events/{id}/categories");

                //check that category information loaded
                if (categories == null)
                {
                    //start of if

                    TempData["Error"] = "unable to load event categories";

                    return RedirectToAction("Events");

                    //end of if
                }


                //prepare information for the razor view
                var model = new EnrolmentsPageViewModel
                {
                    EventID = found_event.EventID,
                    EventName = found_event.EventName,
                    Enrolments = enrolments ?? new List<RaceEnrolment>(),
                    Categories = categories
                };

                //store the selected category filter
                ViewBag.SelectedCategoryID = categoryId;

                //store the selected status filter
                ViewBag.SelectedStatus = status ?? "";

                //display event enrolments
                return View(model);


                //end of try
            }

            catch (Exception e)
            {
                //start of catch

                TempData["Error"] = "unable to connect to the RaceDay API";

                return RedirectToAction("Events");

                //end of catch
            }

            //end of method
        }


        //display captured results for a selected event
        [HttpGet]
        public async Task<IActionResult> Results(int id)
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

            //check event id
            if (id <= 0)
            {
                //start of if

                return RedirectToAction("Events");

                //end of if
            }

            try
            {
                //start of try

                //get event information
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

                //store event date for results view
                ViewBag.EventDate = found_event.EventDate;

                //get current organiser id
                int? organiser_id =
                    HttpContext.Session.GetInt32("UserID");

                //check event ownership
                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //get enrolments belonging to the event
                using var enrolment_response = await Api.Send(
                    HttpContext,
                    HttpMethod.Get,
                    $"api/events/{id}/enrolments"
                );

                //check authentication response
                if (enrolment_response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //start of if

                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");

                    //end of if
                }

                //check access permission
                if (enrolment_response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check if enrolments loaded
                if (!enrolment_response.IsSuccessStatusCode)
                {
                    //start of if

                    TempData["Error"] = "Unable to load event enrolments.";

                    return RedirectToAction("Events");

                    //end of if
                }

                //convert enrolments into C# objects
                var enrolments = await enrolment_response.Content
                    .ReadFromJsonAsync<List<RaceEnrolment>>();

                //create results page model
                var model = new ResultsPageViewModel
                {
                    EventID = id,
                    EventName = found_event.EventName
                };

                //get captured result for each confirmed enrolment
                foreach (var enrolment in enrolments ?? new List<RaceEnrolment>())
                {
                    //start of foreach

                    //only display confirmed enrolments
                    if (enrolment.Status != "Confirmed")
                    {
                        //start of if

                        continue;

                        //end of if
                    }

                    //request the enrolment's result
                    using var result_response = await Api.Send(
                        HttpContext,
                        HttpMethod.Get,
                        $"api/enrolments/{enrolment.EnrolmentID}/result"
                    );

                    //store result if one already exists
                    CapturedResult? captured_result = null;

                    if (result_response.IsSuccessStatusCode)
                    {
                        //start of if

                        captured_result = await result_response.Content
                            .ReadFromJsonAsync<CapturedResult>();

                        //end of if
                    }
                    else if (result_response.StatusCode != HttpStatusCode.NotFound)
                    {
                        //start of else if

                        TempData["Error"] =
                            "Unable to retrieve all race results.";

                        return RedirectToAction("Events");

                        //end of else if
                    }

                    //add enrolment and its result to the page
                    model.Rows.Add(new ResultRow
                    {
                        Enrolment = enrolment,
                        Result = captured_result
                    });

                    //end of foreach
                }

                //display results page
                return View(model);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction("Events");

                //end of catch
            }

            //end of method
        }


        //display the capture result form
        [HttpGet]
        public async Task<IActionResult> CaptureResult(
            int eventId,
            int enrolmentId)
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

            //get event details
            var found_event = await Api.Get<RaceEvent>(
                HttpContext,
                $"api/Events/{eventId}"
            );

            //check event exists
            if (found_event == null)
            {
                //start of if

                return NotFound();

                //end of if
            }

            //check event ownership
            int? organiser_id =
                HttpContext.Session.GetInt32("UserID");

            if (found_event.OrganiserID != organiser_id)
            {
                //start of if

                return Forbid();

                //end of if
            }

            //get event enrolments
            var enrolments = await Api.Get<List<RaceEnrolment>>(
                HttpContext,
                $"api/events/{eventId}/enrolments"
            );

            //confirm the enrolment belongs to this event
            bool valid_enrolment = enrolments?.Any(en =>
                en.EnrolmentID == enrolmentId
                && en.Status == "Confirmed") == true;

            if (!valid_enrolment)
            {
                //start of if

                return NotFound();

                //end of if
            }

            //prepare result form
            var form = new ResultForm
            {
                EventID = eventId,
                EnrolmentID = enrolmentId
            };

            //display capture page
            return View(form);

            //end of method
        }


        //save a participant's race result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CaptureResult(ResultForm form)
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

            //check that the finishing time is valid
            if (form.FinishTime == null ||
                form.FinishTime <= TimeSpan.Zero)
            {
                //start of if

                ModelState.AddModelError(
                    "FinishTime",
                    "Enter a finishing time greater than zero."
                );

                //end of if
            }

            //check form validation
            if (!ModelState.IsValid)
            {
                //start of if

                return View(form);

                //end of if
            }

            try
            {
                //start of try

                //get event information
                var found_event = await Api.Get<RaceEvent>(
                    HttpContext,
                    $"api/Events/{form.EventID}"
                );

                //check if event exists
                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //check organiser ownership
                int? organiser_id =
                    HttpContext.Session.GetInt32("UserID");

                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check enrolment belongs to the selected event
                var enrolments = await Api.Get<List<RaceEnrolment>>(
                    HttpContext,
                    $"api/events/{form.EventID}/enrolments"
                );

                bool valid_enrolment = enrolments?.Any(en =>
                    en.EnrolmentID == form.EnrolmentID
                    && en.Status == "Confirmed") == true;

                if (!valid_enrolment)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //prepare API request
                string route =
                    $"api/enrolments/{form.EnrolmentID}/result?"
                    + RaceDayApi.Query(
                        ("finishTime", form.FinishTime.Value.ToString("c")),
                        ("position", form.Position.ToString(
                            CultureInfo.InvariantCulture))
                    );

                //send result to RaceDay 2 API
                var result = await Api.Write(
                    HttpContext,
                    HttpMethod.Post,
                    route
                );

                //check if result was saved
                if (result.success)
                {
                    //start of if

                    TempData["Success"] =
                        "Race result captured successfully.";

                    return RedirectToAction(
                        "Results",
                        new { id = form.EventID }
                    );

                    //end of if
                }

                //show error on the capture page
                ModelState.AddModelError(
                    "",
                    "The result could not be saved. It may already exist or the position may be taken."
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


        //display the edit result form
        [HttpGet]
        public async Task<IActionResult> EditResult(
            int eventId,
            int enrolmentId,
            int resultId)
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

            //check supplied identifiers
            if (eventId <= 0 || enrolmentId <= 0 || resultId <= 0)
            {
                //start of if

                return BadRequest();

                //end of if
            }

            try
            {
                //start of try

                //get the event information
                var found_event = await Api.Get<RaceEvent>(
                    HttpContext,
                    $"api/Events/{eventId}"
                );

                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //check event ownership
                int? organiser_id =
                    HttpContext.Session.GetInt32("UserID");

                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //get enrolments for the selected event
                var enrolments = await Api.Get<List<RaceEnrolment>>(
                    HttpContext,
                    $"api/events/{eventId}/enrolments"
                );

                //check enrolment belongs to this event
                bool valid_enrolment = enrolments?.Any(e =>
                    e.EnrolmentID == enrolmentId &&
                    e.Status == "Confirmed") == true;

                if (!valid_enrolment)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //get the existing result
                using var response = await Api.Send(
                    HttpContext,
                    HttpMethod.Get,
                    $"api/enrolments/{enrolmentId}/result"
                );

                //check for missing result
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //check request permission
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //check if request succeeded
                response.EnsureSuccessStatusCode();

                //read captured result
                var result = await response.Content
                    .ReadFromJsonAsync<CapturedResult>();

                //verify the result belongs to the enrolment
                if (result == null ||
                    result.ResultID != resultId ||
                    result.EnrolmentID != enrolmentId)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //prepare edit form with existing values
                var form = new EditResultForm
                {
                    ResultID = result.ResultID,
                    EventID = eventId,
                    EnrolmentID = enrolmentId,
                    FinishTime = result.FinishTime,
                    Position = result.Position
                };

                //display edit result page
                return View(form);

                //end of try
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to retrieve the race result from the API.";

                return RedirectToAction(
                    "Results",
                    new { id = eventId }
                );

                //end of catch
            }
            catch (System.Text.Json.JsonException)
            {
                //start of catch

                TempData["Error"] =
                    "The API returned invalid result information.";

                return RedirectToAction(
                    "Results",
                    new { id = eventId }
                );

                //end of catch
            }

            //end of method
        }



        //save corrections to a captured race result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditResult(EditResultForm form)
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

            //check finishing time
            if (form.FinishTime == null ||
                form.FinishTime <= TimeSpan.Zero)
            {
                //start of if

                ModelState.AddModelError(
                    "FinishTime",
                    "Finish time must be greater than zero."
                );

                //end of if
            }

            //validate the form
            if (!ModelState.IsValid)
            {
                //start of if

                return View(form);

                //end of if
            }

            try
            {
                //start of try

                //get selected event
                var found_event = await Api.Get<RaceEvent>(
                    HttpContext,
                    $"api/Events/{form.EventID}"
                );

                if (found_event == null)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //verify event ownership
                int? organiser_id =
                    HttpContext.Session.GetInt32("UserID");

                if (found_event.OrganiserID != organiser_id)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                //verify the selected enrolment
                var enrolments = await Api.Get<List<RaceEnrolment>>(
                    HttpContext,
                    $"api/events/{form.EventID}/enrolments"
                );

                bool valid_enrolment = enrolments?.Any(e =>
                    e.EnrolmentID == form.EnrolmentID &&
                    e.Status == "Confirmed") == true;

                if (!valid_enrolment)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //verify the existing result belongs to the enrolment
                using var existing_response = await Api.Send(
                    HttpContext,
                    HttpMethod.Get,
                    $"api/enrolments/{form.EnrolmentID}/result"
                );

                if (existing_response.StatusCode == HttpStatusCode.NotFound)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                if (existing_response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of if

                    return Forbid();

                    //end of if
                }

                existing_response.EnsureSuccessStatusCode();

                var existing_result = await existing_response.Content
                    .ReadFromJsonAsync<CapturedResult>();

                if (existing_result == null ||
                    existing_result.ResultID != form.ResultID ||
                    existing_result.EnrolmentID != form.EnrolmentID)
                {
                    //start of if

                    return NotFound();

                    //end of if
                }

                //prepare the API query parameters
                string route =
                    $"api/results/{form.ResultID}?"
                    + RaceDayApi.Query(
                        ("finishTime", form.FinishTime.Value.ToString("c")),
                        ("position", form.Position.ToString(
                            CultureInfo.InvariantCulture))
                    );

                //update existing result using the API
                using var response = await Api.Send(
                    HttpContext,
                    HttpMethod.Put,
                    route
                );

                //check whether update succeeded
                if (response.IsSuccessStatusCode)
                {
                    //start of if

                    TempData["Success"] =
                        "Race result updated successfully.";

                    return RedirectToAction(
                        "Results",
                        new { id = form.EventID }
                    );

                    //end of if
                }

                //show conflict message for taken position
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    //start of if

                    ModelState.AddModelError(
                        "Position",
                        "This finishing position is already taken in the category."
                    );

                    //end of if
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    //start of else if

                    return Forbid();

                    //end of else if
                }
                else
                {
                    //start of else

                    ModelState.AddModelError(
                        "",
                        "The result could not be updated. Please check your values."
                    );

                    //end of else
                }

                //display errors without losing the form values
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
            catch (System.Text.Json.JsonException)
            {
                //start of catch

                ModelState.AddModelError(
                    "",
                    "The API returned invalid result information."
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

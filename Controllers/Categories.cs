using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay_2.Models;
using System.Security.Claims;

namespace RaceDay_2.Controllers
{
    //start of namespace
    [Route("api/[controller]")]
    [ApiController]
    public class Categories : ControllerBase
    {
        //start of class

        //database connetcion
        private readonly RaceDay_Context _context;

        public Categories(RaceDay_Context context)
        {
            _context = context;
        }

        //gets the userID of the currently authenticated organiser
        private int get_current_user_id()
        {
            //start of method

            var id_claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (id_claim == null || !int.TryParse(id_claim, out int user_id))
            {
                throw new UnauthorizedAccessException("Could not get current user.");
            }

            return user_id;

            //end of method
        }

        //get
        //list all categories like 5km, 20 km thats is avaibale uder a certain event
        [HttpGet("/api/events/{eventId}/categories")]
        [Authorize]
        public IActionResult GetByEvent (int eventId)
        {
            //start of method

            try
            {
                //start of try

                //check that the event is there
                bool event_exists = _context.Events.Any(e => e.EventID == eventId);

                if (!event_exists)
                {
                    return NotFound(new { message = "Event not found 😢" });
                }

                var categories = _context.Categories.Where(c => c.EventID == eventId).OrderBy(c => c.DistanceKM).Select(c => new
                {
                    c.CategoryID,
                    c.EventID,
                    c.CategoryName,
                    c.DistanceKM,
                    c.MaxParticipants
                }).ToList();

                return Ok(categories);

                //end of try
            }

            catch (Exception e)
            {
                //start of catch

                return BadRequest(e.Message);

                //end of catch
            }
            //end of method
        }

        

        //post
        //adds a new category to an event so that have to enrol into

        [HttpPost("/api/events/{eventId}/categories")]
        [Authorize(Roles = "Organizer")]
        public IActionResult Create(int eventId, string categoryName, decimal distanceKm, int maxParticipants)
        {
            //start of method

            try
            {
                //start of try

                if (distanceKm <= 0)
                {
                    return BadRequest("Distance has to be greater than 0");
                }

                if (maxParticipants <= 0)
                {
                    return BadRequest("MaxParticipants has to be greater than 0");
                }

                int current_user_id = get_current_user_id();

                //check that the event is there and find who owns it
                var owning_event = _context.Events.Find(eventId);

                if (owning_event == null)
                {
                    return NotFound(new { message = "Event not found" });
                }

                //only the organiser who owns the event can add categories to it

                if (owning_event.OrganiserID != current_user_id)
                {
                    return StatusCode(403, new { message = "You do not own this event" });
                }

                var new_category = new Category
                {
                    EventID = eventId,
                    CategoryName = categoryName,
                    DistanceKM = distanceKm,
                    MaxParticipants = maxParticipants
                };

                _context.Categories.Add(new_category);
                _context.SaveChanges();

                //return the created category
                return StatusCode(201, new
                {
                    new_category.CategoryID,
                    new_category.EventID,
                    new_category.CategoryName,
                    new_category.DistanceKM,
                    new_category.MaxParticipants
                });

                //end of try
            }

            catch (Exception e)
            {
                //start of catch

                return BadRequest(e.Message);

                //end of catch
            }

            //end of method
        }

        //end of class
    }
    //end of namesapce
}

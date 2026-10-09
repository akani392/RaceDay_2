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
    public class Events : ControllerBase
    {
        //start of class

        //database connetcion
        private readonly RaceDay_Context _context;

        public Events (RaceDay_Context context)
        {
            _context = context;
        }

        //values that are the events being done
        private static readonly string[] valid_event_types = { "Running", "Walking", "Cycling" };

        //get
        //list all the events so that users are abke to look at which event is avaliable
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            //start of method

            try
            {
                //start of try

                var events = _context.Events.OrderBy(e => e.EventDate).Select(e => new
                {
                    e.EventID,
                    e.OrganiserID,
                    e.EventName,
                    e.EventDate,
                    e.EventType,
                    e.Location,
                    e.Description
                }).ToList();

                return Ok(events);

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

        //gets all the details for a single event
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(int id)
        {
            //start of method

            try
            {
                //start of try

                var found_event = _context.Events.Where(e => e.EventID == id).Select(e => new
                {
                    e.EventID,
                    e.OrganiserID,
                    e.EventName,
                    e.EventDate,
                    e.EventType,
                    e.Location,
                    e.Description,
                    Categories = e.Categories.Select(c => new
                    {
                        c.CategoryID,
                        c.CategoryName,
                        c.DistanceKM,
                        c.MaxParticipants
                    })
                }).SingleOrDefault();

                //check if event is there
                if (found_event == null)
                {
                    return NotFound(new { message = "Event is not found" });
                }

                return Ok(found_event);

                //end of try
            }

            catch (Exception e)
            {
                //start of try

                return BadRequest(e.Message);

                //end of try
            }

            //end of method
        }

        //gets the user id of the current authicated organiser
        private int get_current_user()
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

        //post
        //makes a new event so that logged organiser can strat managiing registrations for it
        [HttpPost]
        [Authorize(Roles = "Organizer")]
        public IActionResult Create(string eventName, DateTime eventDate, string location, string description, string eventType)
        {
            //start  of method

            try
            {
                //start of try
                //running walikng cycling are allowed by the database
                
                if (!valid_event_types.Contains(eventType))
                {
                    return BadRequest("Event name has to be running walking or cycling");
                }

                //get the current logged in organiser user id

                int organizer_id = get_current_user();

                var new_event = new Event
                {
                    OrganiserID = organizer_id,
                    EventName = eventName,
                    EventType = eventType,
                    Location = location,
                    Description = description
                };

                _context.Events.Add(new_event);
                _context.SaveChanges();

                //return the new created event
                return StatusCode(201, new
                {
                    new_event.EventID,
                    new_event.OrganiserID,
                    new_event.EventName,
                    new_event.EventDate,
                    new_event.EventType,
                    new_event.Location,
                    new_event.Description
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

        //Put
        //updates an event, restricted to the organiser who owns it
        [HttpPut("{id}")]
        [Authorize (Roles = "Organizer")]
        public IActionResult Update(int id, string eventName, DateTime eventDate, string location, string description, string eventType)
        {
            //start  of method

            try
            {
                //start of try
                
                //only running, walkking or cycling allowed here
                if (!valid_event_types.Contains(eventType))
                {
                    return BadRequest("EventTpe has to be one of running, walking, cycling");
                }

                int current_user_id = get_current_user();

                //find the event to update
                var found_event = _context.Events.Find(id);

                if (found_event == null)
                {
                    return NotFound(new { message = "Event not found" });
                }

                //only the organiser who owns the event may update it
                if (found_event.OrganiserID != current_user_id)
                {
                    return StatusCode(403, new { message = "You do not own this event" });
                }

                found_event.EventName = eventName;
                found_event.EventDate = eventDate;
                found_event.Location = location;
                found_event.Description = description;
                found_event.EventType = eventType;

                _context.SaveChanges();

                //return the updated event
                return Ok(new
                {
                    found_event.EventID,
                    found_event.OrganiserID,
                    found_event.EventName,
                    found_event.EventDate,
                    found_event.EventType,
                    found_event.Location,
                    found_event.Description
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

        //delete
        



        //end of class

    }
    //end of namespace
}

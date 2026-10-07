using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay_2.Models;

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

        //end of class

    }
    //end of namespace
}

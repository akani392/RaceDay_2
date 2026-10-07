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

        //end of class
    }
    //end of namesapce
}

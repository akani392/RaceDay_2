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
    public class Event_enrolments : ControllerBase
    {

        //start of class

        //database connetcion
        private readonly RaceDay_Context _context;

        public Event_enrolments(RaceDay_Context context)
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

        //post
        //puts the authincated participant into a choosen category so that they are registered to the race

        [HttpPost("/api/enrolments")]
        [Authorize(Roles = "Participant")]
        public IActionResult Create(int categoryId)
        {
            //start of method

            try
            {
                //start of try
                int participant_id = get_current_user_id();

                //check the category exists
                var category = _context.Categories.Find(categoryId);

                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }

                //check if this participant is already enrolled in the category
                int current_enrolments = _context.Enrolments.Count(en => en.CategoryID == categoryId && en.Status == "Confirmed");

                if (current_enrolments >= category.MaxParticipants)
                {
                    return Conflict(new { message = "This category is full" });
                }

                var new_enrolment = new Enrolment
                {
                    ParticipantID = participant_id,
                    CategoryID = categoryId
                };

                _context.Enrolments.Add(new_enrolment);
                _context.SaveChanges();

                //return the new created enrolment
                return StatusCode(201, new
                {
                    new_enrolment.EnrolmentID,
                    new_enrolment.ParticipantID,
                    new_enrolment.CategoryID,
                    new_enrolment.EnrolmentDate,
                    new_enrolment.Status
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

        //get
        //gets all the enrolments belonging to the authenticated participant
        [HttpGet("/api/enrolments/me")]
        [Authorize(Roles = "Participant")]
        public IActionResult GetMine()
        {
            //start of method

            try
            {
                //start of try

                int participant_id = get_current_user_id();

                var enrolments = _context.Enrolments.Where(en => en.ParticipantID == participant_id).OrderByDescending(en => en.EnrolmentDate).Select(en => new
                {
                    en.EnrolmentID,
                    en.CategoryID,
                    en.EnrolmentDate,
                    en.Status
                }).ToList();

                return Ok(enrolments);

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

    //end of namespace
}

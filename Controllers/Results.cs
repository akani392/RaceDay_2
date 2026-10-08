using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_2.Models;
using System.Security.Claims;

namespace RaceDay_2.Controllers
{
    //start of namespace

    [Route("api/[controller]")]
    [ApiController]
    public class Results : ControllerBase
    {
        //start of class

        //database connetcion
        private readonly RaceDay_Context _context;

        public Results(RaceDay_Context context)
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
        //gwts a finish time and position for a certain enrolmet when the race is done
        [HttpPost("api/enrolments/{enrolmentId}/result")]
        [Authorize (Roles = "Organizer")]
        public IActionResult Create (int enrolmentId, TimeSpan finishTime, int position)
        {
            //start of meyhod
            try
            {
                //start of try

                if (position <= 0)
                {
                    return BadRequest("Position has to be greater than 0");
                }

                int current_user_id = get_current_user_id();

                //find the enrollment and the organiser who owns the event
                var enrolment = _context.Enrolments.Include(en => en.Category).ThenInclude(c => c!.Event).SingleOrDefault(en => en.EnrolmentID == enrolmentId);

                if (enrolment == null || enrolment.Category == null || enrolment.Category.Event == null)
                {
                    return NotFound(new { message = "Enrolment not found" });
                }

                //check a result has not been already put in
                bool already_captured = _context.Results.Any(r => r.EnrolmentID == enrolmentId);

                if (already_captured)
                {
                    return Conflict(new { message = "A result already exists for this enrolment" });
                }

                var new_result = new Result
                {
                    EnrolmentID = enrolmentId,
                    CapturedByID = current_user_id,
                    FinishTime = finishTime,
                    Position = position
                };

                _context.Results.Add(new_result);
                _context.SaveChanges();

                //return the new captured result
                return StatusCode(201, new
                {
                    new_result.ResultID,
                    new_result.EnrolmentID,
                    new_result.CapturedByID,
                    new_result.FinishTime,
                    new_result.Position,
                    new_result.DateCaptured
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
        //gwt the reuslt for a certain enrolment
        [HttpGet("/api/enrolments/{enrolmentId}/result")]
        [Authorize]
        public IActionResult GetByEnrolemt(int enrolmentId)
        {
            //start of method

            try
            {
                //start of try

                var result = _context.Results.Where(r => r.EnrolmentID == enrolmentId).Select(r => new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    r.CapturedByID,
                    r.FinishTime,
                    r.Position,
                    r.DateCaptured
                }).SingleOrDefault();

                // check if a result has been put in
                if (result == null)
                {
                    return NotFound(new { message = "No result has been captured yet" });
                }

                return Ok(result);

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

        //put
        //corrects previosly captured result restricted to the organiser who got it

        [HttpPut("/api/results/{id}")]
        [Authorize(Roles = "Organizer")]
        public IActionResult Update (int id, TimeSpan finishTime, int position)
        {
            //start of method

            try
            {
                //start of try

                if (position <= 0)
                {
                    return BadRequest("Position has to be greater than 0");
                }

                int current_user_id = get_current_user_id();

                //fnd who got the result
                var result = _context.Results.Find(id);

                if(result == null)
                {
                    return NotFound(new { message = "Result not gound" });
                }

                //only the organiser who got the result can correct it
                if (result.CapturedByID != current_user_id)
                {
                    return StatusCode(403, new { message = "You did not capture this result" });
                }

                result.FinishTime = finishTime;
                result.Position = position;

                _context.SaveChanges();

                //return the updated result

                return Ok(new
                {
                    result.ResultID,
                    result.FinishTime,
                    result.Position
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
        //list the authenticated participants full personal results history
        [HttpGet("/api/users/me/results")]
        [Authorize(Roles = "Participant")]
        public IActionResult GetMyResults()
        {

            //start of method

            try
            {
                //start of try

                int participant_id = get_current_user_id();

                var results = _context.Results.Where(r => r.Enrolment!.ParticipantID == participant_id).OrderByDescending(r => r.DateCaptured).Select(r = new
                {
                    r.ResultID,
                    r.Enrolment,
                    EventName = r.Enrolment!.Category!.Event!.EventName,
                    CategoryName = r.Enrolment.Category.CategoryName,
                    r.FinishTime,
                    r.Position,
                    r.DateCaptured
                }).ToList();

                return Ok(results);


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

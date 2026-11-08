using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_2.Models;
using System.Security.Claims;

namespace RaceDay_2.Controllers
{
    //start of namespace

    [Route("/api/[controller]")]
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

        //capture a result for a participant after a race
        [HttpPost("/api/enrolments/{enrolmentId}/result")]
        [Authorize(Roles = "Organiser")]
        public IActionResult Create(
            int enrolmentId,
            [FromQuery] TimeSpan finishTime,
            [FromQuery] int position)
        {
            //start of method

            //check finishing time and position
            if (finishTime <= TimeSpan.Zero || position <= 0)
            {
                //start of if

                return BadRequest(new
                {
                    message = "Finish time and position must be greater than zero"
                });

                //end of if
            }

            //get the authenticated organiser
            int current_user_id;

            try
            {
                //start of try

                current_user_id = get_current_user_id();

                //end of try
            }
            catch (UnauthorizedAccessException)
            {
                //start of catch

                return Unauthorized();

                //end of catch
            }

            //find the enrolment and its event
            var enrolment = _context.Enrolments
                .Include(en => en.Category)
                .ThenInclude(c => c!.Event)
                .SingleOrDefault(en => en.EnrolmentID == enrolmentId);

            //check enrolment exists
            if (enrolment?.Category?.Event == null)
            {
                //start of if

                return NotFound(new
                {
                    message = "Enrolment not found"
                });

                //end of if
            }

            //check that the organiser owns the event
            if (enrolment.Category.Event.OrganiserID != current_user_id)
            {
                //start of if

                return StatusCode(403, new
                {
                    message = "You do not own this event"
                });

                //end of if
            }

            //only confirmed participants can receive results
            if (enrolment.Status != "Confirmed")
            {
                //start of if

                return BadRequest(new
                {
                    message = "Enrolment must be confirmed"
                });

                //end of if
            }

            //check if the event has taken place
            if (enrolment.Category.Event.EventDate > DateTime.Now)
            {
                //start of if

                return BadRequest(new
                {
                    message = "Results cannot be captured before the event date"
                });

                //end of if
            }

            //prevent duplicate results
            bool already_captured = _context.Results
                .Any(r => r.EnrolmentID == enrolmentId);

            if (already_captured)
            {
                //start of if

                return Conflict(new
                {
                    message = "A result already exists for this enrolment"
                });

                //end of if
            }

            //prevent duplicate positions in one category
            bool position_taken = _context.Results
                .Any(r =>
                    r.Enrolment!.CategoryID == enrolment.CategoryID
                    && r.Position == position);

            if (position_taken)
            {
                //start of if

                return Conflict(new
                {
                    message = "Position already assigned in this category"
                });

                //end of if
            }

            //create the result
            var new_result = new Result
            {
                EnrolmentID = enrolmentId,
                CapturedByID = current_user_id,
                FinishTime = finishTime,
                Position = position
            };

            //save the result
            _context.Results.Add(new_result);
            _context.SaveChanges();

            //return the captured result
            return StatusCode(201, new
            {
                new_result.ResultID,
                new_result.EnrolmentID,
                new_result.CapturedByID,
                new_result.FinishTime,
                new_result.Position,
                new_result.DateCaptured
            });

            //end of method
        }


        //get

        //get a result for a selected enrolment
        [HttpGet("/api/enrolments/{enrolmentId}/result")]
        [Authorize]
        public IActionResult GetByEnrolemt(int enrolmentId)
        {
            //start of method

            //get the authenticated user
            int current_user_id;

            try
            {
                //start of try

                current_user_id = get_current_user_id();

                //end of try
            }
            catch (UnauthorizedAccessException)
            {
                //start of catch

                return Unauthorized();

                //end of catch
            }

            //get enrolment and the event it belongs to
            var enrolment = _context.Enrolments
                .Include(en => en.Category)
                .ThenInclude(c => c!.Event)
                .SingleOrDefault(en => en.EnrolmentID == enrolmentId);

            //check if enrolment exists
            if (enrolment?.Category?.Event == null)
            {
                //start of if

                return NotFound(new
                {
                    message = "Enrolment not found"
                });

                //end of if
            }

            //get the current user's role
            bool is_organiser = User.IsInRole("Organiser");
            bool is_participant = User.IsInRole("Participant");

            //check if the organiser owns the event
            bool owns_event = is_organiser
                && enrolment.Category.Event.OrganiserID == current_user_id;

            //check if the participant owns this enrolment
            bool owns_enrolment = is_participant
                && enrolment.ParticipantID == current_user_id;

            //reject unauthorised access
            if (!owns_event && !owns_enrolment)
            {
                //start of if

                return StatusCode(403, new
                {
                    message = "You cannot view this result"
                });

                //end of if
            }

            //find the captured result
            var result = _context.Results
                .Where(r => r.EnrolmentID == enrolmentId)
                .Select(r => new
                {
                    r.ResultID,
                    r.EnrolmentID,
                    r.CapturedByID,
                    r.FinishTime,
                    r.Position,
                    r.DateCaptured
                })
                .SingleOrDefault();

            //check if a result has been captured
            if (result == null)
            {
                //start of if

                return NotFound(new
                {
                    message = "No result has been captured yet"
                });

                //end of if
            }

            //return the result
            return Ok(result);

            //end of method
        }


        //put
        //correct a previously captured race result
        [HttpPut("/api/results/{id}")]
        [Authorize(Roles = "Organiser")]
        public IActionResult Update(
            int id,
            [FromQuery] TimeSpan finishTime,
            [FromQuery] int position)
        {
            //start of method

            //check finishing time and position
            if (finishTime <= TimeSpan.Zero || position <= 0)
            {
                //start of if

                return BadRequest(new
                {
                    message = "Finish time and position must be greater than zero."
                });

                //end of if
            }

            //get the logged in organiser
            int current_user_id;

            try
            {
                //start of try

                current_user_id = get_current_user_id();

                //end of try
            }
            catch (UnauthorizedAccessException)
            {
                //start of catch

                return Unauthorized();

                //end of catch
            }

            //find the existing result and its race category
            var result = _context.Results
                .Include(r => r.Enrolment)
                .SingleOrDefault(r => r.ResultID == id);

            //check result exists
            if (result?.Enrolment == null)
            {
                //start of if

                return NotFound(new
                {
                    message = "Result not found."
                });

                //end of if
            }

            //only the organiser who captured it can edit it
            if (result.CapturedByID != current_user_id)
            {
                //start of if

                return StatusCode(403, new
                {
                    message = "You did not capture this result."
                });

                //end of if
            }

            //check if another result has the selected position
            bool position_taken = _context.Results.Any(r =>
                r.ResultID != id &&
                r.Enrolment!.CategoryID == result.Enrolment.CategoryID &&
                r.Position == position
            );

            if (position_taken)
            {
                //start of if

                return Conflict(new
                {
                    message = "This position is already assigned in the category."
                });

                //end of if
            }

            //update the existing result
            result.FinishTime = finishTime;
            result.Position = position;

            //save changes to the database
            _context.SaveChanges();

            //return the updated result
            return Ok(new
            {
                result.ResultID,
                result.EnrolmentID,
                result.FinishTime,
                result.Position
            });

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
                // start of try

                int participant_id = get_current_user_id();

                var results = _context.Results.Where(r => r.Enrolment!.ParticipantID == participant_id).OrderByDescending(r => r.DateCaptured).Select(r => new
                    {
                        r.ResultID,
                        r.EnrolmentID,
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RaceDay_2.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace RaceDay_2.Controllers
{

    //start of namespace
    [Route("/api/[controller]")]
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

        //register a participant into a race category
        [HttpPost("/api/enrolments")]
        [Authorize(Roles = "Participant")]
        public IActionResult Create(int categoryId)
        {
            //start of method

            try
            {
                //start of try

                //get the logged in participant
                int participant_id = get_current_user_id();

                //get category and event information
                var category = _context.Categories
                    .Include(c => c.Event)
                    .SingleOrDefault(c => c.CategoryID == categoryId);

                //check if category exists
                if (category == null || category.Event == null)
                {
                    //start of if

                    return NotFound(new
                    {
                        message = "Category not found"
                    });

                    //end of if
                }

                //check if event is still open for registration
                if (category.Event.EventDate < DateTime.Now)
                {
                    //start of if

                    return BadRequest(new
                    {
                        message = "Registration for this event is closed"
                    });

                    //end of if
                }


                //check if participant already has a confirmed enrolment
                bool already_enrolled = _context.Enrolments
                    .Any(en =>
                        en.ParticipantID == participant_id
                        && en.CategoryID == categoryId
                        && en.Status == "Confirmed");

                if (already_enrolled)
                {
                    //start of if

                    return Conflict(new
                    {
                        message = "You are already registered for this category"
                    });

                    //end of if
                }

                //check that the selected event has not started
                var selected_event = _context.Events
                    .Find(category.EventID);

                if (selected_event == null)
                {
                    //start of if

                    return NotFound(new
                    {
                        message = "Event not found"
                    });

                    //end of if
                }

                if (selected_event.EventDate <= DateTime.Now)
                {
                    //start of if

                    return Conflict(new
                    {
                        message = "Registration for this event has closed"
                    });

                    //end of if
                }


                //count confirmed enrolments
                int current_enrolments = _context.Enrolments
                    .Count(en =>
                        en.CategoryID == categoryId &&
                        en.Status == "Confirmed");

                //check if the category is full
                if (current_enrolments >= category.MaxParticipants)
                {
                    //start of if

                    return Conflict(new
                    {
                        message = "This category is full"
                    });

                    //end of if
                }

                //create the enrolment
                var new_enrolment = new Enrolment
                {
                    ParticipantID = participant_id,
                    CategoryID = categoryId,
                    Status = "Confirmed"
                };

                //save registration to database
                _context.Enrolments.Add(new_enrolment);
                _context.SaveChanges();

                //return successful registration
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
            catch (UnauthorizedAccessException)
            {
                //start of catch

                return Unauthorized();

                //end of catch
            }

            //end of method
        }


        //get

        //get all enrolments belonging to the authenticated participant
        [HttpGet("/api/enrolments/me")]
        [Authorize(Roles = "Participant")]
        public IActionResult GetMine()
        {
            //start of method

            try
            {
                //start of try

                //get the currently logged in participant
                int participant_id = get_current_user_id();

                //get the participant's enrolments and race information
                var enrolments = _context.Enrolments
                    .Where(en => en.ParticipantID == participant_id)
                    .OrderByDescending(en => en.EnrolmentDate)
                    .Select(en => new
                    {
                        en.EnrolmentID,
                        en.ParticipantID,
                        en.CategoryID,
                        en.EnrolmentDate,
                        en.Status,

                        CategoryName = en.Category != null
                            ? en.Category.CategoryName
                            : "",

                        DistanceKM = en.Category != null
                            ? en.Category.DistanceKM
                            : 0m,

                        EventID = en.Category != null
                            ? en.Category.EventID
                            : 0,

                        EventName = en.Category != null
                            && en.Category.Event != null
                            ? en.Category.Event.EventName
                            : "",

                        EventDate = en.Category != null
                            && en.Category.Event != null
                            ? en.Category.Event.EventDate
                            : DateTime.MinValue
                    })
                    .ToList();

                //return the enrolments
                return Ok(enrolments);

                //end of try
            }
            catch (UnauthorizedAccessException)
            {
                //start of catch

                return Unauthorized();

                //end of catch
            }

            //end of method
        }


        //get

        //get all enrolments for an event owned by the organiser
        [HttpGet("/api/events/{eventId}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public IActionResult GetByEvent(int eventId)
        {
            //start of method

            try
            {
                //start of try

                //get the currently logged in organiser
                int current_user_id = get_current_user_id();

                //find the event
                var owning_event = _context.Events.Find(eventId);

                //check if the event exists
                if (owning_event == null)
                {
                    //start of if

                    return NotFound(new
                    {
                        message = "Event not found"
                    });

                    //end of if
                }

                //check that organiser owns the event
                if (owning_event.OrganiserID != current_user_id)
                {
                    //start of if

                    return StatusCode(403, new
                    {
                        message = "You do not own this event"
                    });

                    //end of if
                }

                //get enrolments together with participant and category details
                var enrolments = _context.Enrolments
                    .Where(en => en.Category!.EventID == eventId)
                    .OrderBy(en => en.EnrolmentDate)
                    .Select(en => new
                    {
                        en.EnrolmentID,
                        en.ParticipantID,
                        en.CategoryID,
                        en.EnrolmentDate,
                        en.Status,

                        ParticipantName = en.Participant != null
                            ? en.Participant.FirstName + " " + en.Participant.LastName
                            : "Unknown Participant",

                        CategoryName = en.Category != null
                            ? en.Category.CategoryName
                            : "Unknown Category"
                    })
                    .ToList();

                //return the enrolment list
                return Ok(enrolments);

                //end of try
            }
            catch (Exception e)
            {
                //start of catch

                return BadRequest(new
                {
                    message = e.Message
                });

                //end of catch
            }

            //end of method
        }



        //delete

        //cancel an enrolment belonging to the participant
        [HttpDelete("/api/enrolments/{id}")]
        [Authorize(Roles = "Participant")]
        public IActionResult Delete(int id)
        {
            //start of method

            try
            {
                //start of try

                //get the currently logged in participant
                int current_user_id = get_current_user_id();

                //find the enrolment and associated event
                var enrolment = _context.Enrolments
                    .Where(en => en.EnrolmentID == id)
                    .Select(en => new
                    {
                        Enrolment = en,
                        EventDate = en.Category != null
                            && en.Category.Event != null
                            ? en.Category.Event.EventDate
                            : DateTime.MinValue
                    })
                    .SingleOrDefault();

                //check if enrolment exists
                if (enrolment == null)
                {
                    //start of if

                    return NotFound(new
                    {
                        message = "Enrolment not found"
                    });

                    //end of if
                }

                //check that the participant owns the enrolment
                if (enrolment.Enrolment.ParticipantID != current_user_id)
                {
                    //start of if

                    return StatusCode(403, new
                    {
                        message = "This is not your enrolment"
                    });

                    //end of if
                }

                //check whether enrolment is already cancelled
                if (enrolment.Enrolment.Status == "Cancelled")
                {
                    //start of if

                    return Conflict(new
                    {
                        message = "This enrolment is already cancelled"
                    });

                    //end of if
                }

                //do not allow cancellation after the event starts
                if (enrolment.EventDate <= DateTime.Now)
                {
                    //start of if

                    return Conflict(new
                    {
                        message = "Cannot cancel an enrolment after the event has started"
                    });

                    //end of if
                }

                //check whether a race result exists
                bool has_result = _context.Results
                    .Any(r => r.EnrolmentID == id);

                if (has_result)
                {
                    //start of if

                    return Conflict(new
                    {
                        message = "Cannot cancel an enrolment with a captured result"
                    });

                    //end of if
                }

                //update registration status instead of deleting
                enrolment.Enrolment.Status = "Cancelled";

                //save changes to database
                _context.SaveChanges();

                //return successful cancellation
                return NoContent();

                //end of try
            }
            catch (UnauthorizedAccessException)
            {
                //start of catch

                return Unauthorized();

                //end of catch
            }

            //end of method
        }


        //end of class
    }

    //end of namespace
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay_2.Controllers;
using RaceDay_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RaceDay_2_Test
{
    //start of namespace

    //tests that the wrong role or a missing login is turned away
    //role rules are written as [Authorize] attributes, the web server checks them before
    //a method runs, so here the attributes are read to prove each rule is in place
    //the rules that live inside the methods (who owns what) are tested by calling them
    public class testing_role_access
    {
        //start of class

        //helper 

        //finds the [Authorize] attribute on a method, if there is one
        private AuthorizeAttribute? get_authorize(Type controller_type, string method_name)
        {
            //start of get authorize method

            MethodInfo method = controller_type.GetMethod(method_name)!;

            return method.GetCustomAttribute<AuthorizeAttribute>();

            //end of get authorize method
        }


        //organiser only endpoints 

        //only organisers may create events
        [Fact]
        public void create_event_is_organiser_only()
        {
            //start of create event role method

            var rule = get_authorize(typeof(Events), "Create");

            Assert.NotNull(rule);
            Assert.Equal("Organiser", rule!.Roles);

            //end of create event role method
        }


        //only organisers may update events
        [Fact]
        public void update_event_is_organiser_only()
        {
            //start of update event role method

            var rule = get_authorize(typeof(Events), "Update");

            Assert.NotNull(rule);
            Assert.Equal("Organiser", rule!.Roles);

            //end of update event role method
        }


        //only organisers may delete events
        [Fact]
        public void delete_event_is_organiser_only()
        {
            //start of delete event role method

            var rule = get_authorize(typeof(Events), "Delete");

            Assert.NotNull(rule);
            Assert.Equal("Organiser", rule!.Roles);

            //end of delete event role method
        }


        //only organisers may add categories
        [Fact]
        public void create_category_is_organiser_only()
        {
            //start of create category role method

            var rule = get_authorize(typeof(Categories), "Create");

            Assert.NotNull(rule);
            Assert.Equal("Organiser", rule!.Roles);

            //end of create category role method
        }


        //only organisers may capture results
        [Fact]
        public void capture_result_is_organiser_only()
        {
            //start of capture result role method

            var rule = get_authorize(typeof(Results), "Create");

            Assert.NotNull(rule);
            Assert.Equal("Organiser", rule!.Roles);

            //end of capture result role method
        }


        //only organisers may see the enrolments of an event
        [Fact]
        public void view_event_enrolments_is_organiser_only()
        {
            //start of event enrolments role method

            var rule = get_authorize(typeof(Event_enrolments), "GetByEvent");

            Assert.NotNull(rule);
            Assert.Equal("Organiser", rule!.Roles);

            //end of event enrolments role method
        }


        //participant only endpoints 

        //only participants may enrol
        [Fact]
        public void enrol_is_participant_only()
        {
            //start of enrol role method

            var rule = get_authorize(typeof(Event_enrolments), "Create");

            Assert.NotNull(rule);
            Assert.Equal("Participant", rule!.Roles);

            //end of enrol role method
        }


        //only participants may cancel an enrolment
        [Fact]
        public void cancel_enrolment_is_participant_only()
        {
            //start of cancel role method

            var rule = get_authorize(typeof(Event_enrolments), "Delete");

            Assert.NotNull(rule);
            Assert.Equal("Participant", rule!.Roles);

            //end of cancel role method
        }


        //any logged in user 

        //viewing events needs a login but not a specific role
        [Fact]
        public void view_events_needs_login_but_any_role()
        {
            //start of view events role method

            var rule = get_authorize(typeof(Events), "GetAll");

            Assert.NotNull(rule);
            Assert.True(string.IsNullOrEmpty(rule!.Roles));

            //end of view events role method
        }


        //the profile endpoints are protected for the whole controller
        [Fact]
        public void user_profile_needs_login()
        {
            //start of profile role method

            var rule = typeof(User_Profile).GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(rule);

            //end of profile role method
        }


        //register and login must stay open so people can get an account
        [Fact]
        public void register_and_login_are_public()
        {
            //start of public method

            Assert.Null(get_authorize(typeof(Authentication), "Register"));
            Assert.Null(get_authorize(typeof(Authentication), "Login"));
            Assert.Null(typeof(Authentication).GetCustomAttribute<AuthorizeAttribute>());

            //end of public method
        }


        //-------------------- not logged in --------------------

        //a request with no login cannot create an event
        [Fact]
        public void not_logged_in_cannot_create_event()
        {
            //start of not logged in event method

            var context = test_helper.create_context();

            Events test_events = new Events(context);
            test_helper.set_not_logged_in(test_events);

            var results = test_events.Create("Race", new DateTime(2026, 11, 8), "Soweto", "None", "Running");

            var bad = Assert.IsType<BadRequestObjectResult>(results);
            Assert.Equal("Could not get current user.", bad.Value);
            Assert.Empty(context.Events);

            //end of not logged in event method
        }


        //a request with no login cannot enrol
        [Fact]
        public void not_logged_in_cannot_enrol()
        {
            //start of not logged in enrol method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "organiser@test.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);
            var category = test_helper.add_category(context, saved_event.EventID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_not_logged_in(test_enrolments);

            var results = test_enrolments.Create(category.CategoryID);

            var bad = Assert.IsType<BadRequestObjectResult>(results);
            Assert.Equal("Could not get current user.", bad.Value);
            Assert.Empty(context.Enrolments);

            //end of not logged in enrol method
        }


        //-------------------- wrong person for the data --------------------

        //an organiser cannot see the enrolments of another organiser's event
        [Fact]
        public void organiser_cannot_view_other_organisers_enrolments()
        {
            //start of other organiser enrolments method

            var context = test_helper.create_context();
            var owner = test_helper.add_user(context, 1, "akani@gmail.com");
            var other_organiser = test_helper.add_user(context, 1, "other@test.com");
            var saved_event = test_helper.add_event(context, owner.UserID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, other_organiser.UserID, "Organiser");

            var results = test_enrolments.GetByEvent(saved_event.EventID);

            var forbidden = Assert.IsType<ObjectResult>(results);
            Assert.Equal(403, forbidden.StatusCode);

            //end of other organiser enrolments method
        }


        //an organiser cannot add a category to another organiser's event
        [Fact]
        public void organiser_cannot_add_category_to_other_event()
        {
            //start of other organiser category method

            var context = test_helper.create_context();
            var owner = test_helper.add_user(context, 1, "akani@gmail.com");
            var other_organiser = test_helper.add_user(context, 1, "nadira@gmail.com");
            var saved_event = test_helper.add_event(context, owner.UserID);

            Categories test_categories = new Categories(context);
            test_helper.set_logged_in_user(test_categories, other_organiser.UserID, "Organizer");

            var results = test_categories.Create(saved_event.EventID, "5km", 5.00m, 100);

            var forbidden = Assert.IsType<ObjectResult>(results);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Empty(context.Categories);

            //end of other organiser category method
        }


        //an organiser cannot capture a result for another organiser's event
        [Fact]
        public void organiser_cannot_capture_result_for_other_event()
        {
            //start of other organiser result method

            var context = test_helper.create_context();
            var owner = test_helper.add_user(context, 1, "akani@gmail.com");
            var other_organiser = test_helper.add_user(context, 1, "nadira@gmail.com");
            var participant = test_helper.add_user(context, 2, "Liss@gmail.com");
            var saved_event = test_helper.add_event(context, owner.UserID);
            var category = test_helper.add_category(context, saved_event.EventID);
            var enrolment = test_helper.add_enrolmentt(context, participant.UserID, category.CategoryID);

            Results test_results = new Results(context);
            test_helper.set_logged_in_user(test_results, other_organiser.UserID, "Organiser");

            var results = test_results.Create(enrolment.EnrolmentID, new TimeSpan(1, 30, 0), 1);

            var forbidden = Assert.IsType<ObjectResult>(results);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Empty(context.Results);

            //end of other organiser result method
        }

        //end of class
    }

    //end of namespace
}

using Microsoft.AspNetCore.Mvc;
using RaceDay_2.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay_2_Test
{
    //start of namespace

    //test that organizer are able to create, vir=ew and delete thier events
    public class testinng_events
    {
        //start of class

        //create

        // organizer is able to create an event if it is saved against thier user id
        [Fact]
        public void organizer_creates_event_success()
        {
            //start of method

            var context = test_helper.create_context();
            var organizer = test_helper.add_user(context, 1, "organizer@gmail.com");

            Events test_events = new Events(context);
            test_helper.set_logged_in_user(test_events, organizer.UserID, "Organizer");
            var results = test_events.Create("Midrand Marathon", new DateTime(2026, 11, 8), "Johannesburg", "Annual race", "Running");

            var created = Assert.IsType<ObjectResult>(results);
            Assert.Equal(201, created.StatusCode);

            //event has to be saved and owned by logged organiser

            var saved_event = context.Events.Single();
            Assert.Equal("Midrand Marathon", saved_event.EventName);
            Assert.Equal(organizer.UserID, saved_event.OrganiserID);

            //end of method
        }

        //an event type that the database does not allow is rejected
        [Fact]
        public void create_event_invalid_type_fails()
        {
            //start of method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "oorganizer@gmail.com");

            Events test_events = new Events(context);
            test_helper.set_logged_in_user(test_events, organiser.UserID, "organizer");

            var results = test_events.Create("Bad Race", new DateTime(2026, 11, 8), "Roodeport", "Swimming race", "Swimming");

            Assert.IsType<BadRequestObjectResult>(results);
            Assert.Empty(context.Events);

            //end of method
        }

        //view
        //any logged in user can list all the events
        [Fact]
        public void get_all_events_success()
        {
            //start of method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "oorganizer@gmail.com");

            test_helper.add_event(context, organiser.UserID, "Race 1");
            test_helper.add_event(context, organiser.UserID, "Race 2");

            Events test_events = new Events(context);

            var results = test_events.GetAll();

            var ok = Assert.IsType<OkObjectResult>(results);
            var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
            Assert.Equal(2, list.Cast<object>().Count());

            //end of method
        }

        //one event can be fetched with its categories
        [Fact]
        public void get_event_by_id_success()
        {
            //start of method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "oorganizer@gmail.com");

            var saved_event = test_helper.add_event(context, organiser.UserID);
            test_helper.add_category(context, saved_event.EventID);

            Events test_events = new Events(context);

            var results = test_events.GetById(saved_event.EventID);

            var ok = Assert.IsType<OkObjectResult>(results);
            Assert.Equal("Test Race", test_helper.get_property(ok.Value, "EventName"));

            //end of method
        }





        //an event that does not exist gives an error 404
        [Fact]
        public void get_event_not_found()
        {
            //start of methos

            var context = test_helper.create_context();
            Events test_events = new Events(context);

            var results = test_events.GetById(999);

            Assert.IsType<NotFoundObjectResult>(results);

            //end of method
        }

        //update
        [Fact]
        public void organiser_updates_own_event_success()
        {
            //start of method

            var context = test_helper.create_context();

            var organiser = test_helper.add_user(context, 1, "oorganizer@gmail.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);

            Events test_events = new Events(context);
            test_helper.set_logged_in_user(test_events, organiser.UserID, "Organiser");

            var results = test_events.Update(saved_event.EventID, "Renmaeed race", new DateTime(2027, 1, 1), "Pretoria", "New description", "Cycling");
            Assert.IsType<OkObjectResult>(results);

            var updated_event = context.Events.Single();
            Assert.Equal("Renamed race", updated_event.EventType);

            //end of method
        }

        //an organiser can not update an event that belongs to someone else
        [Fact]
        public void organiser_updates_other_event_forbidden()
        {
            //start of method

            var context = test_helper.create_context();
            var owner = test_helper.add_user(context, 1, "makes@gmail.com");
            var other_organiser = test_helper.add_user(context, 1, "does@gmail.com");

            var saved_event = test_helper.add_event(context, owner.UserID);
            Events test_events = new Events(context);

            test_helper.set_logged_in_user(test_events, other_organiser.UserID, "Organiser");

            //end of method
        }

        //updating an event that does not exist gives a 404
        [Fact]
        public void update_event_not_found()
        {
            //start of update not found method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "organiser@test.com");

            Events test_events = new Events(context);
            test_helper.set_logged_in_user(test_events, organiser.UserID, "Organiser");

            var results = test_events.Update(999, "Race", new DateTime(2027, 1, 1), "Pretoria", "None", "Running");

            Assert.IsType<NotFoundObjectResult>(results);

            //end of update not found method
        }

        //delete 

        //the owning organiser can delete their event
        [Fact]
        public void organiser_deletes_own_event_success()
        {
            //start of delete own method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "organiser@test.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);

            Events test_events = new Events(context);
            test_helper.set_logged_in_user(test_events, organiser.UserID, "Organiser");

            var results = test_events.Delete(saved_event.EventID);

            Assert.IsType<NoContentResult>(results);
            Assert.Empty(context.Events);

            //end of delete own method
        }

        //an organiser cannot delete an event that belongs to someone else
        [Fact]
        public void organiser_deletes_other_event_forbidden()
        {
            //start of delete other method

            var context = test_helper.create_context();
            var owner = test_helper.add_user(context, 1, "owner@test.com");
            var other_organiser = test_helper.add_user(context, 1, "other@test.com");
            var saved_event = test_helper.add_event(context, owner.UserID);

            Events test_events = new Events(context);
            test_helper.set_logged_in_user(test_events, other_organiser.UserID, "Organiser");

            var results = test_events.Delete(saved_event.EventID);

            var forbidden = Assert.IsType<ObjectResult>(results);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Single(context.Events);

            //end of delete other method
        }

        //an event that still has categories cannot be deleted
        [Fact]
        public void delete_event_with_categories_fails()
        {
            //start of delete with categories method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "organiser@test.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);
            test_helper.add_category(context, saved_event.EventID);

            Events test_events = new Events(context);
            test_helper.set_logged_in_user(test_events, organiser.UserID, "Organiser");

            var results = test_events.Delete(saved_event.EventID);

            Assert.IsType<ConflictObjectResult>(results);
            Assert.Single(context.Events);

            //end of delete with categories method
        }

        //end of class

    }

    //end of namespace
}

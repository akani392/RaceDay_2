using Microsoft.AspNetCore.Mvc;
using RaceDay_2.Controllers;
using RaceDay_2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RaceDay_2_Test
{
    //start of namespace

    //tests that Participants can enrol and that the enrolment is correctly recorded
    public class testing_enrolments
    {
        //start of class

        //enrol 

        //a participant can enrol in a category and the enrolment is saved correctly
        [Fact]
        public void participant_enrols_success()
        {
            //start of enrol method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "akani@gmail.com");
            var participant = test_helper.add_user(context, 2, "nadira@gmail.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);
            var category = test_helper.add_category(context, saved_event.EventID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, participant.UserID, "Participant");

            var results = test_enrolments.Create(category.CategoryID);

            var created = Assert.IsType<ObjectResult>(results);
            Assert.Equal(201, created.StatusCode);

            //the link between the participant and the category must be recorded
            var saved_enrolment = context.Enrolments.Single();
            Assert.Equal(participant.UserID, saved_enrolment.ParticipantID);
            Assert.Equal(category.CategoryID, saved_enrolment.CategoryID);
            Assert.Equal("Confirmed", saved_enrolment.Status);

            //end of enrol method
        }


        //enrolling in a category that does not exist gives a 404
        [Fact]
        public void enrol_unknown_category_fails()
        {
            //start of unknown category method

            var context = test_helper.create_context();
            var participant = test_helper.add_user(context, 2, "nadira@gmail.com");

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, participant.UserID, "Participant");

            var results = test_enrolments.Create(999);

            Assert.IsType<NotFoundObjectResult>(results);
            Assert.Empty(context.Enrolments);

            //end of unknown category method
        }


        //the same participant cannot enrol in the same category twice
        [Fact]
        public void enrol_twice_fails()
        {
            //start of enrol twice method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "akani@gmail.com");
            var participant = test_helper.add_user(context, 2, "nadira@gmail.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);
            var category = test_helper.add_category(context, saved_event.EventID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, participant.UserID, "Participant");

            test_enrolments.Create(category.CategoryID);
            var results = test_enrolments.Create(category.CategoryID);

            Assert.IsType<ConflictObjectResult>(results);
            Assert.Single(context.Enrolments);

            //end of enrol twice method
        }


        //a full category cannot take any more participants
        [Fact]
        public void enrol_full_category_fails()
        {
            //start of full category method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "akani@gmail.com");
            var first_runner = test_helper.add_user(context, 2, "first@test.com");
            var second_runner = test_helper.add_user(context, 2, "second@test.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);

            //this category only has room for one participant
            var category = test_helper.add_category(context, saved_event.EventID, 1);
            test_helper.add_enrolmentt(context, first_runner.UserID, category.CategoryID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, second_runner.UserID, "Participant");

            var results = test_enrolments.Create(category.CategoryID);

            Assert.IsType<ConflictObjectResult>(results);
            Assert.Single(context.Enrolments);

            //end of full category method
        }


        //view 

        //a participant only sees their own enrolments
        [Fact]
        public void participant_views_only_own_enrolments()
        {
            //start of view own method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "akani@gmail.com");
            var first_runner = test_helper.add_user(context, 2, "first@test.com");
            var second_runner = test_helper.add_user(context, 2, "second@test.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);
            var category = test_helper.add_category(context, saved_event.EventID);

            test_helper.add_enrolmentt(context, first_runner.UserID, category.CategoryID);
            test_helper.add_enrolmentt(context, second_runner.UserID, category.CategoryID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, first_runner.UserID, "Participant");

            var results = test_enrolments.GetMine();

            var ok = Assert.IsType<OkObjectResult>(results);
            var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
            Assert.Single(list.Cast<object>());

            //end of view own method
        }


        //the owning organiser can see all the enrolments for their event
        [Fact]
        public void organiser_views_event_enrolments_success()
        {
            //start of organiser view method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "akani@gmail.com");
            var first_runner = test_helper.add_user(context, 2, "first@test.com");
            var second_runner = test_helper.add_user(context, 2, "second@test.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);
            var category = test_helper.add_category(context, saved_event.EventID);

            test_helper.add_enrolmentt(context, first_runner.UserID, category.CategoryID);
            test_helper.add_enrolmentt(context, second_runner.UserID, category.CategoryID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, organiser.UserID, "Organiser");

            var results = test_enrolments.GetByEvent(saved_event.EventID);

            var ok = Assert.IsType<OkObjectResult>(results);
            var list = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
            Assert.Equal(2, list.Cast<object>().Count());

            //end of organiser view method
        }


        //cancel 

        //a participant can cancel their own enrolment
        [Fact]
        public void participant_cancels_own_enrolment_success()
        {
            //start of cancel own method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "akani@gmail.com");
            var participant = test_helper.add_user(context, 2, "nadira@gmail.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);
            var category = test_helper.add_category(context, saved_event.EventID);
            var enrolment = test_helper.add_enrolmentt(context, participant.UserID, category.CategoryID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, participant.UserID, "Participant");

            var results = test_enrolments.Delete(enrolment.EnrolmentID);

            Assert.IsType<NoContentResult>(results);
            Assert.Empty(context.Enrolments);

            //end of cancel own method
        }


        //a participant cannot cancel someone else's enrolment
        [Fact]
        public void participant_cancels_other_enrolment_forbidden()
        {
            //start of cancel other method

            var context = test_helper.create_context();
            var organiser = test_helper.add_user(context, 1, "akani@gmail.com");
            var owner_runner = test_helper.add_user(context, 2, "owner@test.com");
            var other_runner = test_helper.add_user(context, 2, "other@test.com");
            var saved_event = test_helper.add_event(context, organiser.UserID);
            var category = test_helper.add_category(context, saved_event.EventID);
            var enrolment = test_helper.add_enrolmentt(context, owner_runner.UserID, category.CategoryID);

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, other_runner.UserID, "Participant");

            var results = test_enrolments.Delete(enrolment.EnrolmentID);

            var forbidden = Assert.IsType<ObjectResult>(results);
            Assert.Equal(403, forbidden.StatusCode);
            Assert.Single(context.Enrolments);

            //end of cancel other method
        }


        //cancelling an enrolment that does not exist gives a 404
        [Fact]
        public void cancel_unknown_enrolment_fails()
        {
            //start of cancel unknown method

            var context = test_helper.create_context();
            var participant = test_helper.add_user(context, 2, "nadira@gmail.com");

            Event_enrolments test_enrolments = new Event_enrolments(context);
            test_helper.set_logged_in_user(test_enrolments, participant.UserID, "Participant");

            var results = test_enrolments.Delete(999);

            Assert.IsType<NotFoundObjectResult>(results);

            //end of cancel unknown method
        }

        //end of class
    }

    //end of namespace
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay_2.Controllers;
using RaceDay_2.Models;
using System.Security.Claims;


namespace RaceDay_2_Test
{
    //start of namespace

    //fake login service so the login and logout methods can run without a real web server
    public class fake_auth_service : IAuthenticationService
    {
        //start of class

        public bool signed_in = false;
        public bool signed_out = false;
        public ClaimsPrincipal? signed_in_principal = null;

        public Task <AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        public Task ChallengeAsync (HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            return Task.CompletedTask;
        }

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            return Task.CompletedTask;
        }

        public Task SignInAsync (HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            signed_in = true;
            signed_in_principal = principal;
            return Task.CompletedTask;
        }

        public Task SignOutAsync (HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            signed_out = true;
            return Task.CompletedTask;
        }
        //end of class
    }

    //helper methods shared by all the test classess
    public static class test_helper
    {
        //start of class
        //gets a new brand emoty in memoery database test role needs the sql script'

        public static RaceDay_Context create_context()
        {
            //start of create context method

            var options = new DbContextOptionsBuilder<RaceDay_Context>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            var context = new RaceDay_Context(options);

            context.Roles.Add(new Roles { RoleID = 1, RoleName = "Organizer" });
            context.Roles.Add(new Roles { RoleID = 2, RoleName = "Participant" });

            context.SaveChanges();
            return context;

            //end of crete context method
        }

        //adds a user to test the database, the password is hashed
        public static User add_user (RaceDay_Context context, int role_id, string email, string password = "Password123")
        {
            //start of add user method

            var user = new User
            {
                RoleID = role_id,
                FirstName = "Test",
                LastName = "User",
                Email = email,
                PasswordHash = Password_Helper.hash_password(password)
            };

            context.Users.Add(user);
            context.SaveChanges();

            return user;

            //end of add user method
        }

        //adds an event owned by the givine organiser
        public static Event add_event(RaceDay_Context context, int organizer_id, string name = "Test Race")
        {
            //start of add event method

            var new_event = new Event
            {
                OrganiserID = organizer_id,
                EventName = name,
                EventDate = new DateTime(2026, 12, 18),
                EventType = "Running",
                Location = "Johannesburg",
                Description = "Test event"
            };

            context.Events.Add(new_event);
            context.SaveChanges();

            return new_event;

            //end of add event method
        }

        //add a category  to an event
        public static Category add_category(RaceDay_Context context, int event_id, int max_participants = 10)
        {
            //start of add category method

            var category = new Category
            {
                EventID = event_id,
                CategoryName = "10km",
                DistanceKM = 10.00m,
                MaxParticipants = max_participants
            };

            context.Categories.Add(category);
            context.SaveChanges();

            return category;

            //end of add category method
        }

        //adds a confired enrolment for a participant
        public static Enrolment add_enrolmentt (RaceDay_Context context, int participant_id, int category_id)
        {
            //start of add enrolment method

            var enrolment = new Enrolment
            {
                ParticipantID = participant_id,
                CategoryID = category_id,
                Status = "Confirmed"
            };

            context.Enrolments.Add(enrolment);
            context.SaveChanges();

            return enrolment;

            //end of add enrolment method
        }

        //makes the controller acts like the user is logged in the id and role are stored in claims, same way has login
        public static void set_logged_in_user(ControllerBase controller, int user_id, string role_name)
        {
            //start of method 

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user_id.ToString()),
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.Role, role_name)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };



            //end of method
        }

        //makes the controller act like noby is logged in
        public static void set_not_logged_in (ControllerBase controller)
        {
            //start of method

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            //end of method
        }

        //gives the controller the fake login service login and log out is tested
        public static fake_auth_service attach_fake_auth(ControllerBase controller)
        {
            //start of method

            var fake = new fake_auth_service();

            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService>(fake);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = services.BuildServiceProvider()
                }
            };

            return fake;

            //end of method
        }

        //reads 1 proptery from a annomous objeytcs the controllers return
        public static object? get_property (object? value, string property_name)
        {
            //start of method

            return value!.GetType().GetProperty(property_name)!.GetValue(value);

            //end of method
        }

        //end of class

    }

    //end of namespace
}
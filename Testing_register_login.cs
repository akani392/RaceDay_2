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

    public class Testing_register_login()
    {
        //start of class

        //test for the register, login and logout endpoints
        [Fact]
        public void register_organiser_success()
        {
            //start of metgod

            //creating instance
            var context = test_helper.create_context();
            Authentication test_auth = new Authentication(context);

            //get the result
            var results = test_auth.Register("Akani", "Mbambo", "akani@gmail.com", "password123", 1);

            //ujse assert to compare
            var created = Assert.IsType<ObjectResult>(results);
            Assert.Equal(201, created.StatusCode);
            Assert.Equal("akani@gmail.com", test_helper.get_property(created.Value, "Email"));
            Assert.Equal(1, test_helper.get_property(created.Value, "RoleID"));

            //the user must the database
            Assert.Single(context.Users);

            //end of method
        }

        //new particpant is able to register
        [Fact]
        public void register_participant_success()
        {

            //start of method

            //creating instance
            var context = test_helper.create_context();
            Authentication test_auth = new Authentication(context);

            //get the result
            var results = test_auth.Register("Nadira", "Diedricks", "nadira@gmail.com", "password456", 1);

            //ujse assert to compare
            var created = Assert.IsType<ObjectResult>(results);
            Assert.Equal(201, created.StatusCode);
            Assert.Equal("nadira@gmail.com", test_helper.get_property(created.Value, "Email"));
            Assert.Equal(1, test_helper.get_property(created.Value, "RoleID"));

            //the user must the database
            Assert.Single(context.Users);

            //end of method

        } 

        //passsword has to be saved hashed and must not be sent back
        [Fact]
        public void register_stores_hashed_password()
        {
            //start of method

            //start of register hashed method
            var context = test_helper.create_context();
            Authentication test_auth = new Authentication(context);

            string test_password = "password123";
            var results = test_auth.Register("Akani", "Mbambo", "akani@gmail.com", test_password, 1);
            var created = Assert.IsType<ObjectResult>(results);

            //the password in the database is not the normal password
            var saved_user = context.Users.Single();
            Assert.NotEqual(test_password, saved_user.PasswordHash);
            Assert.DoesNotContain(test_password, saved_user.PasswordHash);

            //response does not have the password or the hash
            Assert.Null(created.Value!.GetType().GetProperty("PasswordHash"));
            Assert.Null(created.Value!.GetType().GetProperty("Password"));

            //end of method
        }

        //a role that does not exxist has to be rejected
        [Fact]
        public void register_invalid_role_fails()
        {
            //start of method

            var context = test_helper.create_context();
            Authentication test_auth = new Authentication(context);

            var results = test_auth.Register("Bad", "Role", "bad@test.com", "Passwords1234", 99);

            Assert.IsType<BadRequestObjectResult>(results);
            Assert.Empty(context.Users);

            //end of method
        }

        //same can not be repated
        [Fact]
        public void register_duplicate_email_fails()
        {
            //start of method

            var context = test_helper.create_context();
            Authentication test_auth = new Authentication(context);

            test_auth.Register("First", "User", "same@test.com", "Passwords1234", 1);
            var results = test_auth.Register("Second", "User", "same@test.com", "Passwords1234", 2);

            var bad = Assert.IsType<BadRequestObjectResult>(results);
            Assert.Equal("A user with this email already exits.", bad.Value);

            Assert.IsType<BadRequestObjectResult>(results);
            Assert.Single(context.Users);

            //end of method
        }

        //login
        //correct email and password logs the user in and signs the cookie
        [Fact]
        public void correct_login()
        {
            //start of method

            var context = test_helper.create_context();

            test_helper.add_user(context, 1, "akani@gmail.com", "password123");

            Authentication test_auth = new Authentication(context);
            var fake_auth = test_helper.attach_fake_auth(test_auth);
            var results = test_auth.Login("akani@gmail.com", "password123");

            var ok = Assert.IsType<OkObjectResult>(results);
            Assert.Equal("Organizer", test_helper.get_property(ok.Value, "RoleName"));
            Assert.Equal("akani@gmail.com", test_helper.get_property(ok.Value, "Email"));

            //the session cookie has to be created
            Assert.True(fake_auth.signed_in);

            //end of method
        }

        //cookie has to hole the user id and role 
        [Fact]
        public void login_stores_id_and_role_in_session()
        {
            //start of method

            var context = test_helper.create_context();
            var saved_user = test_helper.add_user(context, 2, "lis@gmail.com", "password123");


            Authentication test_auth = new Authentication(context);
            var fake_auth = test_helper.attach_fake_auth(test_auth);

            test_auth.Login("Lis@gmail.com", "password123");

            var principal = fake_auth.signed_in_principal;
            Assert.Equal(saved_user.UserID.ToString(), principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            Assert.True(principal.IsInRole("Participant"));

            //end of method
        }

        //wrong password is rejected and no cookie is created
        [Fact]
        public void login_wrong_password_fails()
        {
            //start of method

            var context = test_helper.create_context();
            test_helper.add_user(context, 1, "akani@gmail.com", "password123");

            Authentication test_auth = new Authentication(context);
            var fake_auth = test_helper.attach_fake_auth(test_auth);

            var results = test_auth.Login("akani@gmail.com", "Wrong Password");

            Assert.IsType<UnauthorizedObjectResult>(results);
            Assert.False(fake_auth.signed_in);

            //end of method
        }

        //an email that is not registered is rejected
        [Fact]
        public void login_unknown_email_fails()
        {
            //start of method

            var context = test_helper.create_context();
            Authentication test_auth = new Authentication(context);
            var fake_auth = test_helper.attach_fake_auth(test_auth);


            var results = test_auth.Login("nothing@gmail.com", "Wrong Password");

            Assert.IsType<UnauthorizedObjectResult>(results);
            Assert.False(fake_auth.signed_in);

            //end of method
        }

        //logging out clears the cookie
        [Fact]
        public void logout_success()
        {
            //start of method

            var context = test_helper.create_context();
            Authentication test_auth = new Authentication(context);
            var fake_auth = test_helper.attach_fake_auth(test_auth);

            var results = test_auth.Logout();

            Assert.IsType<OkObjectResult>(results);
            Assert.False(fake_auth.signed_in);
            //end of method
        }
    }

    //end of namespace
}

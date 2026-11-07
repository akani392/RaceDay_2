using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
using System.Text.Json;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public class AccountController : BaseController
    {
        //start of class

        //constructor for acoount controller
        public AccountController (RaceDayApi api) : base(api)
        {

        }

        //process the user login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> Login (LoginForm form)
        {
            //start of method

            //validate user input
            if (!ModelState.IsValid)
            {
                //start of if

                return View(form);

                //end of if
            }

            // prepare login parameters for the api
            string route = "api/Authentication/login?" + RaceDayApi.Query (("email", form.Email), ("password", form.Password));

            //swnd login request
            using var response = await Api.Send(HttpContext, HttpMethod.Post, route);

            //check if login failed
            if (!response.IsSuccessStatusCode)
            {
                //start of if statement

                ModelState.AddModelError("", "Invalid email or password");

                return View(form);

                //end of if
            }
            //gwt login response
            JsonElement login = await response.Content.ReadFromJsonAsync<JsonElement>();

            //check authhencation cookie if it has been returened
            if (!response.Headers.TryGetValues("Set-Cookie", out var header))
            {
                //start of if

                ModelState.AddModelError("", "Authentication cookie has not been found");
                return View(form);

                //end of if
            }

            //get the RaceDay authentication ookie
            string? cookie = header.FirstOrDefault(value => value.StartsWith("RaceDay.Auth=", StringComparison.Ordinal))?.Split(';')[0];

            //check cokie
            if (string.IsNullOrEmpty(cookie))
            {
                //start of if

                ModelState.AddModelError("", "Authentication could not be completeed");
                return View(form);

                //end of if
            }

            //clear old session information
            HttpContext.Session.Clear();


            //clear old session information
            HttpContext.Session.Clear();

            //get authentication information from API response
            string role_name = login.GetProperty("roleName").GetString()?.Trim() ?? "";
            string first_name = login.GetProperty("firstName").GetString() ?? "";
            int user_id = login.GetProperty("userID").GetInt32();

            //save authentication information
            HttpContext.Session.SetString("ApiCookie", cookie);
            HttpContext.Session.SetString("Role", role_name);
            HttpContext.Session.SetString("Name", first_name);
            HttpContext.Session.SetInt32("UserID", user_id);

            //display debugging information
            System.Diagnostics.Debug.WriteLine(
                $"Login Role: [{role_name}], " +
                $"User ID: {user_id}, " +
                $"Cookie Stored: {!string.IsNullOrEmpty(cookie)}"
            );




            //get the user's role
            string? role = HttpContext.Session.GetString("Role")?.Trim();

            //check if the user is an organiser
            if (string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase))
            {
                //start of if

                return RedirectToAction("Index", "Organiser");

                //end of if
            }

            //check if the user is a participant
            if (string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase))
            {
                //start of if

                //redirect participant to their personal dashboard
                return RedirectToAction("Dashboard", "Participant");

                //end of if
            }

            //clear the session if the role is invalid
            HttpContext.Session.Clear();

            //display an error instead of redirecting to the wrong dashboard
            ModelState.AddModelError(
                "",
                "Your account role could not be recognised. Please try logging in again."
            );

            return View(form);

            //end of method


        }

        //show login page
        [HttpGet]
        public IActionResult Login()
        {
            //start of method

            return View(new LoginForm());

            //end of method
        }

        //show registraion page
        [HttpGet]
        public IActionResult Register()
        {
            //start of method

            return View(new RegisterForm());

            //end of method
        }

        //process user registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterForm form)
        {
            //start of method

            //validate the user input
            if (!ModelState.IsValid)
            {
                //start of if

                return View(form);

                //end of if
            }

            //get the registration parameters
            string route = "api/Authentication/register?" + RaceDayApi.Query(
                ("firstName", form.FirstName),
                ("lastName", form.LastName),
                ("email", form.Email),
                ("password", form.Password),
                ("roleId", form.RoleID.ToString())
                );

            //send registraction request
            var result = await Api.Write(HttpContext, HttpMethod.Post, route);
            Feedback(result);

            //send successful registration to login
            if (result.success)
            {
                //syart of statment

                return RedirectToAction(nameof(Login));

                //end of statment
            }

            return View(form);

            //end of method
        }

        //log the user out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            //start f method

            //tell api to logout the user
            try
            {
                //start of try

                //tell the api to logout the user
                await Api.Write(HttpContext,HttpMethod.Post,"api/Authentication/logout");


                //end of try
            }

            //finally, used to that the code runs regardless
            finally
            {
                //start of finally

                //clear the mvx session
                HttpContext.Session.Clear();

                //end of finallyy
            }

            //return to the homepage
            return RedirectToAction("Index", "Home");

            //end of method
        }


        //temporarily check the current login session
        [HttpGet]
        public IActionResult DebugSession()
        {
            //start of method

            //get information stored in the MVC session
            string? user_role =
                HttpContext.Session.GetString("Role");

            string? user_name =
                HttpContext.Session.GetString("Name");

            string? auth_cookie =
                HttpContext.Session.GetString("ApiCookie");

            int? user_id =
                HttpContext.Session.GetInt32("UserID");

            //return safe debugging information
            return Json(new
            {
                Role = user_role,
                Name = user_name,
                UserID = user_id,
                HasApiCookie = !string.IsNullOrWhiteSpace(auth_cookie),
                SessionID = HttpContext.Session.Id
            });

            //end of method
        }


        //end of class
    }

    //end of na,espace
}

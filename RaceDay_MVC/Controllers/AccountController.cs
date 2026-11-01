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

            //save authenticatioon information
            HttpContext.Session.SetString("ApiCookie", cookie);
            HttpContext.Session.SetString("Role", login.GetProperty("roleName").GetString() ?? "");
            HttpContext.Session.SetString("Name", login.GetProperty("firstName").GetString() ?? "");
            HttpContext.Session.SetInt32("UserID", login.GetProperty("userID").GetInt32());

            //get the user's role
            string? role = HttpContext.Session.GetString("Role");

            //send organiser to organiser dashbaord
            if (role == "Organiser")
            {
                //start of if

                return RedirectToAction("Index", "Organiser");

                //end of if
            }

            //redirect particpant to partcpant dashboard
            return RedirectToAction("Index", "Particpant");
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

                await Api.Write(HttpContext, HttpMethod.Post, "api/Authrntication/logout");


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

        //end of class
    }

    //end of na,espace
}

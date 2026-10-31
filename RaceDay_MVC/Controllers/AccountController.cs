using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
using System.Text.Json;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public class AccountController : HomeController
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

        //end of class
    }

    //end of na,espace
}

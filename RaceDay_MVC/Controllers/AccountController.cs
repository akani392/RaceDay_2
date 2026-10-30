using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;

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
        }

        //end of class
    }

    //end of na,espace
}

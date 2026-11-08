
using Microsoft.AspNetCore.Mvc;
using RaceDay_MVC.Models;
using RaceDay_MVC.Services;
using System.Net;
using System.Net.Http.Json;

namespace RaceDay_MVC.Controllers
{
    //start of namespace

    public class ProfilePicturesController : BaseController
    {
        //start of class

        private readonly ProfileImageStore _images;

        public ProfilePicturesController(
            RaceDayApi api,
            ProfileImageStore images) : base(api)
        {
            //start of constructor

            _images = images;

            //end of constructor
        }

        //display the logged in user's profile picture
        [HttpGet]
        public IActionResult Current()
        {
            //start of method

            int? user_id = HttpContext.Session.GetInt32("UserID");

            string? api_cookie =
                HttpContext.Session.GetString("ApiCookie");

            //check the user session
            if (user_id == null ||
                string.IsNullOrWhiteSpace(api_cookie))
            {
                return NotFound();
            }

            //look up the saved picture
            var image = _images.FindImage(user_id.Value);

            if (image == null)
            {
                return NotFound();
            }

            //prevent the browser from showing an old image
            Response.Headers.CacheControl = "no-store";

            return PhysicalFile(
                image.Value.path,
                image.Value.content_type
            );

            //end of method
        }

        //upload or replace a profile picture
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(3 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile? image)
        {
            //start of method

            string? role = HttpContext.Session.GetString("Role");

            //check whether the user has a valid role
            if (!HasRole("Participant") && !HasRole("Organiser"))
            {
                return RedirectToAction("Login", "Account");
            }

            //choose the correct profile page
            string profile_controller =
                HasRole("Organiser") ? "Organiser" : "Participant";

            if (image == null)
            {
                TempData["Error"] = "Please select a profile picture.";

                return RedirectToAction("Profile", profile_controller);
            }

            try
            {
                //start of try

                //confirm the authenticated account with the API
                using var response = await Api.Send(
                    HttpContext,
                    HttpMethod.Get,
                    "api/User_Profile/GetUser"
                );

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction("Login", "Account");
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return Forbid();
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        "Unable to verify your account.";

                    return RedirectToAction(
                        "Profile",
                        profile_controller
                    );
                }

                //get the authenticated user's ID
                var user = await response.Content
                    .ReadFromJsonAsync<UserProfile>();

                if (user == null || user.UserID <= 0)
                {
                    TempData["Error"] =
                        "Your account could not be verified.";

                    return RedirectToAction(
                        "Profile",
                        profile_controller
                    );
                }

                //save the profile picture
                await _images.SaveImage(
                    user.UserID,
                    image,
                    HttpContext.RequestAborted
                );

                TempData["Success"] =
                    "Profile picture uploaded successfully! 📸";

                return RedirectToAction(
                    "Profile",
                    profile_controller
                );

                //end of try
            }
            catch (InvalidDataException exception)
            {
                //start of catch

                TempData["Error"] = exception.Message;

                return RedirectToAction(
                    "Profile",
                    profile_controller
                );

                //end of catch
            }
            catch (HttpRequestException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to connect to the RaceDay API.";

                return RedirectToAction(
                    "Profile",
                    profile_controller
                );

                //end of catch
            }
            catch (IOException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to save the picture. Please try again.";

                return RedirectToAction(
                    "Profile",
                    profile_controller
                );

                //end of catch
            }
            catch (System.Text.Json.JsonException)
            {
                //start of catch

                TempData["Error"] =
                    "Unable to read your account information.";

                return RedirectToAction(
                    "Profile",
                    profile_controller
                );

                //end of catch
            }

            //end of method
        }

        //end of class
    }

    //end of namespace
}

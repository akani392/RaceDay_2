using System.Net;
using System.Text.Json;
using System.Text.Json; 

namespace RaceDay_MVC.Services
{
    //start of namespace

    public class RaceDayApi
    {

        //start of class

        //http client for communication with RaceDay_2 Api
        private readonly HttpClient _client;

        // configure json serialzation options
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        //comstuctor for Api service
        public RaceDayApi(HttpClient client)
        {
            //start of constructor

            _client = client;

            //end of constructor
        }


        //send an http request to the api
        public async Task<HttpResponseMessage> Send(HttpContext context,HttpMethod method,string route,object? body = null)
        {
            //start of method

            //create the http request
            var request = new HttpRequestMessage(method, route);

            //get authentication cookie from session
            string? cookie = context.Session.GetString("ApiCookie");

            //if the attach cookie is avaliable
            if (!string.IsNullOrEmpty(cookie))
            {
                //start of if

                request.Headers.TryAddWithoutValidation("Cookie", cookie);

                //end of if
            }

            //get json data if a request body is there
            if (body != null)
            {
                //start of if

                request.Content = JsonContent.Create(body);

                //end of if
            }

            //send request to the api
            return await _client.SendAsync(request);

            //end of method
        }

        //get the information from the api
        public async Task<T?> Get<T>(HttpContext context, string route)
        {
            //start of method

            using var response = await Send(context, HttpMethod.Get, route);

            //check if the api request was successful
            if (!response.IsSuccessStatusCode)
            {
                //start of if

                return default;

                //end of if
            }

            //check if api responses have information
            if (response.Content == null)
            {
                //start of if 

                return default;

                //end of if
            }

            //convert json response into a C# object
            return await response.Content.ReadFromJsonAsync<T>(Json);

            //end of method
        }

        //send create, update, delete requests to the api
        public async Task<(bool success, string message)> Write(HttpContext context, HttpMethod method, string route, object? body = null)
        {
            //start of method

            using var response = await Send(context, method, route, body);

            //check if the resquest was successful
            if (response.IsSuccessStatusCode)
            {
                //start of if

                return (true, "Saved Successfuly");

                //end of if
            }

            //if to check if user has permission
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                //start of if 

                return (false, "You do not have permission for this action");

                //end of if
            }

            return (false, "The request is not completed. Check your inputs and try again");



            //end of method
        }

        //convert parameters into an encidede url query string
        public static string Query(params (string key, string value)[] values)
        {
            //start of method

            return string.Join("&", values.Select(parameter => Uri.UnescapeDataString(parameter.key) + "=" + Uri.EscapeDataString(parameter.value)));

            //end of method
        }

        //end of class

    }

    //end of namespace
}

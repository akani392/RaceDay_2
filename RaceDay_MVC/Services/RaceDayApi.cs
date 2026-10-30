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
        public async Task<HttpRequestMessage> Send(HttpContext context,HttpMethod method,string route,object? body = null)
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

        //end of class

    }

    //end of namespace
}

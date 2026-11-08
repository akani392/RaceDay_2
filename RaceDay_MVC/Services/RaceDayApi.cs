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


        //send create update and delete requests to the API
        public async Task<(bool success, string message)> Write(
            HttpContext context,
            HttpMethod method,
            string route,
            object? body = null)
        {
            //start of method

            //send the request to the API
            using var response = await Send(
                context,
                method,
                route,
                body
            );

            //check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                //start of if

                return (true, "Your changes were saved successfully.");

                //end of if
            }

            //handle expired or missing authentication
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //start of if

                return (false, "Your login has expired. Please log in again.");

                //end of if
            }

            //handle permission errors
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                //start of if

                return (false, "You do not have permission to perform this action.");

                //end of if
            }

            //handle missing resources
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                //start of if

                return (false, "The selected item could not be found.");

                //end of if
            }

            //handle validation errors and conflicts
            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Conflict)
            {
                //start of if

                //default message if the API has no usable explanation
                string fallback_message = response.StatusCode ==
                    HttpStatusCode.Conflict
                    ? "This action conflicts with existing race information."
                    : "Please check the information you entered.";

                //read structured validation messages
                try
                {
                    //start of try

                    string content = await response.Content.ReadAsStringAsync();

                    //only attempt to parse a reasonably sized response
                    if (!string.IsNullOrWhiteSpace(content) &&
                        content.Length <= 4096)
                    {
                        //start of if

                        using var document = JsonDocument.Parse(content);

                        //check if the API returned a message object
                        if (document.RootElement.ValueKind == JsonValueKind.Object &&
                            document.RootElement.TryGetProperty(
                                "message",
                                out JsonElement message))
                        {
                            //start of if

                            if (message.ValueKind == JsonValueKind.String)
                            {
                                //start of if

                                string? api_message = message.GetString();

                                if (!string.IsNullOrWhiteSpace(api_message) &&
                                    api_message.Length <= 200)
                                {
                                    //start of if

                                    return (false, api_message);

                                    //end of if
                                }

                                //end of if
                            }

                            //end of if
                        }

                        //end of if
                    }

                    //end of try
                }
                catch (JsonException)
                {
                    //start of catch

                    //use the safe fallback for non-JSON API responses

                    //end of catch
                }

                return (false, fallback_message);

                //end of if
            }

            //handle unexpected server errors
            if ((int)response.StatusCode >= 500)
            {
                //start of if

                return (false,
                    "The RaceDay server encountered a problem. Please try again.");

                //end of if
            }

            //handle all other unsuccessful responses
            return (false,
                "The request could not be completed. Please try again.");

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

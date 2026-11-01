namespace RaceDay_MVC.Models
{
    //start of namesoace
    public class ErrorViewModel
    {
        //start of class

        //saves identifier when error happens
        public string? RequestId { get; set; }

        //check if a request is avaiable
        public bool ShowRequestId
        {
            // start of property

            get
            {
                //start of get method

                return !string.IsNullOrEmpty(RequestId);

                //end of get method
            }

            //end of properties
        }

        //end of class
    }

    //end of namespace
}

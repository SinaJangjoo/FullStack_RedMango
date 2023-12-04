using System.Net;

namespace RedMango_API.Models
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            ErrorMessages = new List<string>();      //To handle an empty List of ErrorMessages
        }


        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public List<string> ErrorMessages { get; set; }
        public object Result { get; set; }          // We set object because we don't know the type of those values
    }
}

using System.Net;

namespace Any2Remote.Windows.AdminClient.Core.Exceptions
{
    public class ServerRequestException : Exception
    {
        public HttpStatusCode StatusCode { get; init; }

        public ServerRequestException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public ServerRequestException(HttpStatusCode statusCode, Exception innerException) 
            : base(innerException.Message, innerException)
        {
            StatusCode = statusCode;
        }

        public ServerRequestException(HttpStatusCode statusCode) : base()
        {
            StatusCode = statusCode;
        }
    }
}

using System.Net;

namespace Core.Exceptions
{
    public class HttpBaseServerException : HTTPBaseException
    {
        public HttpBaseServerException(
            string message,
            string errorCode,
            HttpStatusCode statusCode,
            List<string>? errorDetails = null,
            string? stepName = null
        ) : base(message, errorCode, statusCode, errorDetails, stepName)
        {
            if ((int)statusCode < 500 || (int)statusCode >= 600)
                throw new ArgumentException("HttpBaseServerException supports only 5xx status codes.", nameof(statusCode));
        }
    }
}

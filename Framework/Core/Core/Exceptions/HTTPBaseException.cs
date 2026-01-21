
using System.Net;

namespace Core.Exceptions
{
    public class HTTPBaseException : BaseException
    {
        public HttpStatusCode StatusCode { get; }
        public List<string>? ErrorDetails { get; }
        public string? StepName { get; }
        public string ErrorCode { get; }
        public bool IsDownStreamError { get; }

        public HTTPBaseException(string message, string errorCode, HttpStatusCode statusCode, List<string>? errorDetails = null, string? stepName = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorDetails = errorDetails ?? new List<string>();
            StepName = stepName;
            ErrorCode = errorCode;
        }

        public HTTPBaseException(string message, string errorCode, HttpStatusCode statusCode, List<string>? errorDetails = null, string? stepName = null,
            bool isDownStreamError = false)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorDetails = errorDetails ?? new List<string>();
            StepName = stepName;
            ErrorCode = errorCode;
            IsDownStreamError = isDownStreamError;
        }
    }
}

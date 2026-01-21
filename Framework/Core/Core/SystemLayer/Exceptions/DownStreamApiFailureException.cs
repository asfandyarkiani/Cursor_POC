using Core.Constants;
using Core.Exceptions;
using System.Net;

namespace Core.SystemLayer.Exceptions
{
    public class DownStreamApiFailureException : HTTPBaseException
    {
        public DownStreamApiFailureException(
            HttpStatusCode statusCode,
            (string ErrorCode, string Message)? error = null,
            List<string>? errorDetails = null,
            string? stepName = null)
            : base(
                error?.Message ?? ErrorCodes.DOWNSTREAM_API_FAILED.Message,
                error?.ErrorCode ?? ErrorCodes.DOWNSTREAM_API_FAILED.ErrorCode,
                statusCode,
                errorDetails,
                stepName)
        {
        }
    }
}

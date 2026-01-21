
using Core.Constants;
using System.Net;

namespace Core.Exceptions
{
    public class RequestValidationFailureException : HTTPBaseException
    {
        public RequestValidationFailureException(
            (string ErrorCode, string Message)? error = null,
            List<string>? errorDetails = null,
            string? stepName = null
        ) : base(
                error?.Message ?? ErrorCodes.REQ_VALIDATION_FAILURE.Message,
                error?.ErrorCode ?? ErrorCodes.REQ_VALIDATION_FAILURE.ErrorCode,
                HttpStatusCode.BadRequest,
                errorDetails,
                stepName)
        { }
    }
}
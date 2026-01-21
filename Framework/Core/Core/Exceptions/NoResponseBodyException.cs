using Core.Constants;
using System.Net;

namespace Core.Exceptions
{
    public class NoResponseBodyException : HttpBaseServerException
    {
        public NoResponseBodyException(
            (string ErrorCode, string Message)? error = null,
            List<string>? errorDetails = null,
            string? stepName = null)
            : base(
                error?.Message ?? ErrorCodes.RES_BODY_MISSING_OR_EMPTY.Message,
                error?.ErrorCode ?? ErrorCodes.RES_BODY_MISSING_OR_EMPTY.ErrorCode,
                HttpStatusCode.InternalServerError, 
                errorDetails, 
                stepName
            )
        {

        }
    }
}
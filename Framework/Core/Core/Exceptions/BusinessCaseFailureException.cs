using Core.Constants;
using System.Net;

namespace Core.Exceptions
{
    public class BusinessCaseFailureException : HTTPBaseException
    {
        public BusinessCaseFailureException(
            (string ErrorCode, string Message)? error = null,
            List<string>? errorDetails = null,
            string? stepName = null
        ) : base(
            error?.Message ?? ErrorCodes.BUSINESSCASE_FAILURE.Message, 
            error?.ErrorCode ?? ErrorCodes.BUSINESSCASE_FAILURE.ErrorCode,
            HttpStatusCode.Conflict,
            errorDetails, 
            stepName
        )
        {

        }

        public BusinessCaseFailureException(
            HttpStatusCode httpCode,
            (string ErrorCode, string Message)? error = null,
            List<string>? errorDetails = null,
            string? stepName = null
        ) : base(
            error?.Message ?? ErrorCodes.BUSINESSCASE_FAILURE.Message,
            error?.ErrorCode ?? ErrorCodes.BUSINESSCASE_FAILURE.ErrorCode,
            httpCode,
            errorDetails,
            stepName
            )
        {
            if (httpCode != HttpStatusCode.Conflict && (int)httpCode != 422) 
            {
                throw new ArgumentException(
                    "BusinessCaseFailureException only supports status codes 409 (Conflict) or 422 (Unprocessable Entity).",
                    nameof(httpCode)
                );
            }
        }

    }
}

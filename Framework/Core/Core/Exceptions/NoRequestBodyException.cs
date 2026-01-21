
using Core.Constants;
using Grpc.Core;
using System.Net;

namespace Core.Exceptions
{
    public class NoRequestBodyException : HTTPBaseException
    {
        public NoRequestBodyException(
            (string ErrorCode, string Message)? error = null,
            List<string>? errorDetails = null,
            string? stepName = null)
            : base(
                error?.Message ?? ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message,
                error?.ErrorCode ?? ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.ErrorCode,
                HttpStatusCode.BadRequest,
                errorDetails,
                stepName)
        { }
    }
}

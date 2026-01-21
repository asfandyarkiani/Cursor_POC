using Core.Constants;
using System.Net;

namespace Core.Exceptions
{
    public class NotFoundException : HTTPBaseException
    {
        public NotFoundException(
            (string ErrorCode, string Message)? error = null,
            List<string>? errorDetails = null,
            string? stepName = null)
            : base(
                  error?.Message ?? ErrorCodes.RECORD_NOT_FOUND.Message,
                  error?.ErrorCode ?? ErrorCodes.RECORD_NOT_FOUND.ErrorCode,
                  HttpStatusCode.NotFound, 
                  errorDetails, 
                  stepName
            )
        { }
    }
}

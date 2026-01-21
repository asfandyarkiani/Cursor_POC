using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using System.Net;

namespace Core.ProcessLayer.Exceptions
{
    public class PassThroughHttpException : HTTPBaseException
    {
        public PassThroughHttpException(BaseResponseDTO response, HttpStatusCode statusCode)
            : base(
                message: response.Message,
                errorCode: response.ErrorCode,
                statusCode: statusCode,
                errorDetails: response.GetErrors(),
                stepName: response.GetStepName(),
                isDownStreamError: response.IsDownStreamError
            )
        {

        }
    }
}

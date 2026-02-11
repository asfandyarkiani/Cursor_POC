using CAFMSystem.Constants;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.DTO.HandlerDTOs.GetLocationsByDtoDTO;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.FSI.AtomicHandlers;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CAFMSystem.Implementations.FSI.Handlers
{
    public class GetLocationsByDtoHandler : IBaseHandler<GetLocationsByDtoReqDTO>
    {
        private readonly ILogger<GetLocationsByDtoHandler> _logger;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsByDtoAtomicHandler;

        public GetLocationsByDtoHandler(
            ILogger<GetLocationsByDtoHandler> logger,
            GetLocationsByDtoAtomicHandler getLocationsByDtoAtomicHandler)
        {
            _logger = logger;
            _getLocationsByDtoAtomicHandler = getLocationsByDtoAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(GetLocationsByDtoReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Get Locations By Dto");
            
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new DownStreamApiFailureException(
                    statusCode: HttpStatusCode.Unauthorized,
                    error: ErrorConstants.SYS_AUTHENT_0002,
                    errorDetails: ["SessionId is required but not found in context"],
                    stepName: "GetLocationsByDtoHandler.cs / HandleAsync");
            }
            
            GetLocationsByDtoHandlerReqDTO atomicRequest = new GetLocationsByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                UnitCode = request.UnitCode
            };
            
            HttpResponseSnapshot response = await _getLocationsByDtoAtomicHandler.Handle(atomicRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"GetLocationsByDto failed with status: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.SYS_LOCGET_0001,
                    errorDetails: [$"GetLocationsByDto API failed. Status: {response.StatusCode}. Response: {response.Content}"],
                    stepName: "GetLocationsByDtoHandler.cs / HandleAsync");
            }
            
            GetLocationsByDtoApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<GetLocationsByDtoApiResDTO>(response.Content!);
            
            if (apiResponse == null || apiResponse.Envelope?.Body?.GetLocationsByDtoResponse?.GetLocationsByDtoResult?.LocationDto == null 
                || apiResponse.Envelope.Body.GetLocationsByDtoResponse.GetLocationsByDtoResult.LocationDto.Count < 1)
            {
                throw new NotFoundException(
                    ErrorConstants.SYS_LOCGET_0002,
                    [$"No location found for UnitCode: {request.UnitCode}"],
                    "GetLocationsByDtoHandler.cs / HandleAsync");
            }
            
            _logger.Info("[System Layer]-Completed Get Locations By Dto");
            
            return new BaseResponseDTO(
                message: InfoConstants.GET_LOCATION_SUCCESS,
                data: GetLocationsByDtoResDTO.Map(apiResponse),
                errorCode: null);
        }
    }
}

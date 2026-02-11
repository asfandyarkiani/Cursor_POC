using CAFMSystem.Constants;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.DTO.HandlerDTOs.GetInstructionSetsByDtoDTO;
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
    public class GetInstructionSetsByDtoHandler : IBaseHandler<GetInstructionSetsByDtoReqDTO>
    {
        private readonly ILogger<GetInstructionSetsByDtoHandler> _logger;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsByDtoAtomicHandler;

        public GetInstructionSetsByDtoHandler(
            ILogger<GetInstructionSetsByDtoHandler> logger,
            GetInstructionSetsByDtoAtomicHandler getInstructionSetsByDtoAtomicHandler)
        {
            _logger = logger;
            _getInstructionSetsByDtoAtomicHandler = getInstructionSetsByDtoAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(GetInstructionSetsByDtoReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Get Instruction Sets By Dto");
            
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new DownStreamApiFailureException(
                    statusCode: HttpStatusCode.Unauthorized,
                    error: ErrorConstants.SYS_AUTHENT_0002,
                    errorDetails: ["SessionId is required but not found in context"],
                    stepName: "GetInstructionSetsByDtoHandler.cs / HandleAsync");
            }
            
            GetInstructionSetsByDtoHandlerReqDTO atomicRequest = new GetInstructionSetsByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                SubCategory = request.SubCategory
            };
            
            HttpResponseSnapshot response = await _getInstructionSetsByDtoAtomicHandler.Handle(atomicRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"GetInstructionSetsByDto failed with status: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.SYS_INSGET_0001,
                    errorDetails: [$"GetInstructionSetsByDto API failed. Status: {response.StatusCode}. Response: {response.Content}"],
                    stepName: "GetInstructionSetsByDtoHandler.cs / HandleAsync");
            }
            
            GetInstructionSetsByDtoApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsByDtoApiResDTO>(response.Content!);
            
            if (apiResponse == null || apiResponse.Envelope?.Body?.GetInstructionSetsByDtoResponse?.GetInstructionSetsByDtoResult?.FINFILEDto == null 
                || apiResponse.Envelope.Body.GetInstructionSetsByDtoResponse.GetInstructionSetsByDtoResult.FINFILEDto.Count < 1)
            {
                throw new NotFoundException(
                    ErrorConstants.SYS_INSGET_0002,
                    [$"No instruction set found for SubCategory: {request.SubCategory}"],
                    "GetInstructionSetsByDtoHandler.cs / HandleAsync");
            }
            
            _logger.Info("[System Layer]-Completed Get Instruction Sets By Dto");
            
            return new BaseResponseDTO(
                message: InfoConstants.GET_INSTRUCTION_SET_SUCCESS,
                data: GetInstructionSetsByDtoResDTO.Map(apiResponse),
                errorCode: null);
        }
    }
}

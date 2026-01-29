using CAFMSystem.Constants;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.DTO.HandlerDTOs.CreateBreakdownTaskDTO;
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
    public class CreateBreakdownTaskHandler : IBaseHandler<CreateBreakdownTaskReqDTO>
    {
        private readonly ILogger<CreateBreakdownTaskHandler> _logger;
        private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskAtomicHandler;

        public CreateBreakdownTaskHandler(
            ILogger<CreateBreakdownTaskHandler> logger,
            CreateBreakdownTaskAtomicHandler createBreakdownTaskAtomicHandler)
        {
            _logger = logger;
            _createBreakdownTaskAtomicHandler = createBreakdownTaskAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateBreakdownTaskReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Breakdown Task");
            
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.Error("SessionId not found in RequestContext");
                throw new DownStreamApiFailureException(
                    statusCode: HttpStatusCode.Unauthorized,
                    error: ErrorConstants.SYS_AUTHENT_0002,
                    errorDetails: ["SessionId is required but not found in context"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
            }
            
            CreateBreakdownTaskHandlerReqDTO atomicRequest = new CreateBreakdownTaskHandlerReqDTO
            {
                SessionId = sessionId,
                ServiceRequestNumber = request.ServiceRequestNumber,
                ReporterName = request.ReporterName,
                ReporterEmail = request.ReporterEmail,
                ReporterPhoneNumber = request.ReporterPhoneNumber,
                Description = request.Description,
                BuildingId = request.BuildingId,
                LocationId = request.LocationId,
                CategoryId = request.CategoryId,
                DisciplineId = request.DisciplineId,
                PriorityId = request.PriorityId,
                InstructionId = request.InstructionId,
                ContractId = request.ContractId,
                CallerSourceId = request.CallerSourceId,
                ScheduledDateUtc = request.ScheduledDateUtc,
                RaisedDateUtc = request.RaisedDateUtc
            };
            
            HttpResponseSnapshot response = await _createBreakdownTaskAtomicHandler.Handle(atomicRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.Error($"CreateBreakdownTask failed with status: {response.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)response.StatusCode,
                    error: ErrorConstants.SYS_TSKCRT_0001,
                    errorDetails: [$"CreateBreakdownTask API failed. Status: {response.StatusCode}. Response: {response.Content}"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
            }
            
            CreateBreakdownTaskApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<CreateBreakdownTaskApiResDTO>(response.Content!);
            
            if (apiResponse == null || apiResponse.Envelope?.Body?.CreateBreakdownTaskResponse?.CreateBreakdownTaskResult?.TaskId == null)
            {
                throw new NoResponseBodyException(
                    error: ErrorConstants.SYS_TSKCRT_0002,
                    errorDetails: ["CreateBreakdownTask succeeded but no TaskId returned"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
            }
            
            _logger.Info("[System Layer]-Completed Create Breakdown Task");
            
            return new BaseResponseDTO(
                message: InfoConstants.CREATE_BREAKDOWN_TASK_SUCCESS,
                data: CreateBreakdownTaskResDTO.Map(apiResponse),
                errorCode: null);
        }
    }
}

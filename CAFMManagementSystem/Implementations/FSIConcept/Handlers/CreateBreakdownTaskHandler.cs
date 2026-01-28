using CAFMManagementSystem.Constants;
using CAFMManagementSystem.DTO.AtomicHandlerDTOs;
using CAFMManagementSystem.DTO.CreateBreakdownTaskDTO;
using CAFMManagementSystem.DTO.DownstreamDTOs;
using CAFMManagementSystem.Helper;
using CAFMManagementSystem.Implementations.FSIConcept.AtomicHandlers;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.Helpers;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CAFMManagementSystem.Implementations.FSIConcept.Handlers
{
    public class CreateBreakdownTaskHandler : IBaseHandler<CreateBreakdownTaskReqDTO>
    {
        private readonly ILogger<CreateBreakdownTaskHandler> _logger;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsByDtoAtomicHandler;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsByDtoAtomicHandler;
        private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskAtomicHandler;
        private readonly CreateEventAtomicHandler _createEventAtomicHandler;
        
        public CreateBreakdownTaskHandler(
            ILogger<CreateBreakdownTaskHandler> logger,
            GetLocationsByDtoAtomicHandler getLocationsByDtoAtomicHandler,
            GetInstructionSetsByDtoAtomicHandler getInstructionSetsByDtoAtomicHandler,
            CreateBreakdownTaskAtomicHandler createBreakdownTaskAtomicHandler,
            CreateEventAtomicHandler createEventAtomicHandler)
        {
            _logger = logger;
            _getLocationsByDtoAtomicHandler = getLocationsByDtoAtomicHandler;
            _getInstructionSetsByDtoAtomicHandler = getInstructionSetsByDtoAtomicHandler;
            _createBreakdownTaskAtomicHandler = createBreakdownTaskAtomicHandler;
            _createEventAtomicHandler = createEventAtomicHandler;
        }
        
        public async Task<BaseResponseDTO> HandleAsync(CreateBreakdownTaskReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Breakdown Task");
            
            string? sessionId = RequestContext.GetSessionId();
            
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BaseException(ErrorConstants.CAF_SESSIO_0001);
            }
            
            // Step 1: Get Location IDs
            HttpResponseSnapshot locationResponse = await GetLocationsByDtoFromDownstream(request, sessionId);
            
            if (!locationResponse.IsSuccessStatusCode)
            {
                _logger.Error($"GetLocationsByDto failed: {locationResponse.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)locationResponse.StatusCode,
                    error: ErrorConstants.CAF_GETLOC_0001,
                    errorDetails: [$"CAFM GetLocationsByDto API failed. Status: {locationResponse.StatusCode}. Response: {locationResponse.Content}"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync"
                );
            }
            
            GetLocationsByDtoApiResDTO? locationData = SOAPHelper.DeserializeSoapResponse<GetLocationsByDtoApiResDTO>(locationResponse.Content!);
            
            if (locationData == null)
            {
                throw new NoResponseBodyException(
                    error: ErrorConstants.CAF_GETLOC_0002,
                    errorDetails: ["CAFM GetLocationsByDto returned empty response"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync"
                );
            }
            
            // Step 2: Get Instruction Set ID
            HttpResponseSnapshot instructionResponse = await GetInstructionSetsByDtoFromDownstream(request, sessionId);
            
            if (!instructionResponse.IsSuccessStatusCode)
            {
                _logger.Error($"GetInstructionSetsByDto failed: {instructionResponse.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)instructionResponse.StatusCode,
                    error: ErrorConstants.CAF_GETINS_0001,
                    errorDetails: [$"CAFM GetInstructionSetsByDto API failed. Status: {instructionResponse.StatusCode}. Response: {instructionResponse.Content}"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync"
                );
            }
            
            GetInstructionSetsByDtoApiResDTO? instructionData = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsByDtoApiResDTO>(instructionResponse.Content!);
            
            if (instructionData == null)
            {
                throw new NoResponseBodyException(
                    error: ErrorConstants.CAF_GETINS_0002,
                    errorDetails: ["CAFM GetInstructionSetsByDto returned empty response"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync"
                );
            }
            
            // Step 3: Create Breakdown Task
            HttpResponseSnapshot createResponse = await CreateBreakdownTaskInDownstream(
                request, 
                sessionId, 
                locationData.LocationId ?? string.Empty,
                locationData.BuildingId ?? string.Empty,
                instructionData.InstructionId ?? string.Empty
            );
            
            if (!createResponse.IsSuccessStatusCode)
            {
                _logger.Error($"CreateBreakdownTask failed: {createResponse.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)createResponse.StatusCode,
                    error: ErrorConstants.CAF_CRTTSK_0001,
                    errorDetails: [$"CAFM CreateBreakdownTask API failed. Status: {createResponse.StatusCode}. Response: {createResponse.Content}"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync"
                );
            }
            
            CreateBreakdownTaskApiResDTO? createData = SOAPHelper.DeserializeSoapResponse<CreateBreakdownTaskApiResDTO>(createResponse.Content!);
            
            if (createData == null)
            {
                throw new NoResponseBodyException(
                    error: ErrorConstants.CAF_CRTTSK_0002,
                    errorDetails: ["CAFM CreateBreakdownTask returned empty response"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync"
                );
            }
            
            // Step 4: Conditional Event Linking (only if recurrence == "Y")
            if (request.TicketDetails != null && request.TicketDetails.Recurrence == "Y")
            {
                HttpResponseSnapshot eventResponse = await CreateEventInDownstream(
                    sessionId,
                    createData.BreakdownTaskId ?? string.Empty
                );
                
                if (!eventResponse.IsSuccessStatusCode)
                {
                    _logger.Warn($"CreateEvent failed: {eventResponse.StatusCode} - Continuing with task creation");
                }
                else
                {
                    _logger.Info("Event created and linked successfully");
                }
            }
            else
            {
                _logger.Info("Skipping event creation (recurrence != Y)");
            }
            
            _logger.Info("[System Layer]-Completed Create Breakdown Task");
            return new BaseResponseDTO(
                message: InfoConstants.CREATE_BREAKDOWN_TASK_SUCCESS,
                data: CreateBreakdownTaskResDTO.Map(createData),
                errorCode: null
            );
        }
        
        private async Task<HttpResponseSnapshot> GetLocationsByDtoFromDownstream(CreateBreakdownTaskReqDTO request, string sessionId)
        {
            GetLocationsByDtoHandlerReqDTO atomicRequest = new GetLocationsByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                PropertyName = request.PropertyName,
                UnitCode = request.UnitCode
            };
            
            return await _getLocationsByDtoAtomicHandler.Handle(atomicRequest);
        }
        
        private async Task<HttpResponseSnapshot> GetInstructionSetsByDtoFromDownstream(CreateBreakdownTaskReqDTO request, string sessionId)
        {
            GetInstructionSetsByDtoHandlerReqDTO atomicRequest = new GetInstructionSetsByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                CategoryName = request.CategoryName,
                SubCategory = request.SubCategory
            };
            
            return await _getInstructionSetsByDtoAtomicHandler.Handle(atomicRequest);
        }
        
        private async Task<HttpResponseSnapshot> CreateBreakdownTaskInDownstream(
            CreateBreakdownTaskReqDTO request,
            string sessionId,
            string locationId,
            string buildingId,
            string instructionId)
        {
            // Format dates according to Boomi scripting logic
            string scheduledDateUtc = FormatScheduledDateUtc(
                request.TicketDetails?.ScheduledDate ?? string.Empty,
                request.TicketDetails?.ScheduledTimeStart ?? string.Empty
            );
            
            string raisedDateUtc = FormatRaisedDateUtc(request.TicketDetails?.RaisedDateUtc ?? string.Empty);
            
            CreateBreakdownTaskHandlerReqDTO atomicRequest = new CreateBreakdownTaskHandlerReqDTO
            {
                SessionId = sessionId,
                ReporterName = request.ReporterName,
                ReporterEmail = request.ReporterEmail,
                ReporterPhoneNumber = request.ReporterPhoneNumber,
                CallId = request.ServiceRequestNumber,
                CategoryId = "TODO_CATEGORY_ID", // TODO: Get from lookup subprocess
                DisciplineId = "TODO_DISCIPLINE_ID", // TODO: Get from lookup subprocess
                PriorityId = "TODO_PRIORITY_ID", // TODO: Get from lookup subprocess
                BuildingId = buildingId,
                LocationId = locationId,
                InstructionId = instructionId,
                LongDescription = request.Description,
                ScheduledDateUtc = scheduledDateUtc,
                RaisedDateUtc = raisedDateUtc,
                ContractId = "TODO_CONTRACT_ID", // TODO: Get from defined property
                CallerSourceId = "EQ+" // Source system identifier
            };
            
            return await _createBreakdownTaskAtomicHandler.Handle(atomicRequest);
        }
        
        private async Task<HttpResponseSnapshot> CreateEventInDownstream(string sessionId, string breakdownTaskId)
        {
            CreateEventHandlerReqDTO atomicRequest = new CreateEventHandlerReqDTO
            {
                SessionId = sessionId,
                BreakdownTaskId = breakdownTaskId
            };
            
            return await _createEventAtomicHandler.Handle(atomicRequest);
        }
        
        private string FormatScheduledDateUtc(string scheduledDate, string scheduledTimeStart)
        {
            if (string.IsNullOrWhiteSpace(scheduledDate) || string.IsNullOrWhiteSpace(scheduledTimeStart))
            {
                return string.Empty;
            }
            
            try
            {
                string fullDateTime = $"{scheduledDate}T{scheduledTimeStart}Z";
                DateTime date = DateTime.Parse(fullDateTime);
                string formattedDate = date.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
                return formattedDate;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to format scheduled date: {ex.Message}");
                return string.Empty;
            }
        }
        
        private string FormatRaisedDateUtc(string raisedDateUtc)
        {
            if (string.IsNullOrWhiteSpace(raisedDateUtc))
            {
                return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
            }
            
            try
            {
                DateTime date = DateTime.Parse(raisedDateUtc);
                string formattedDate = date.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
                return formattedDate;
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to format raised date: {ex.Message}");
                return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z");
            }
        }
    }
}

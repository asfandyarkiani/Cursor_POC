using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using sys_cafm_mgmt.Constants;
using sys_cafm_mgmt.DTO.AtomicHandlerDTOs;
using sys_cafm_mgmt.DTO.CreateBreakdownTaskDTO;
using sys_cafm_mgmt.DTO.DownstreamDTOs;
using sys_cafm_mgmt.Helper;
using sys_cafm_mgmt.Implementations.CAFM.AtomicHandlers;
using System.Net;

namespace sys_cafm_mgmt.Implementations.CAFM.Handlers
{
    public class CreateBreakdownTaskHandler : IBaseHandler<CreateBreakdownTaskReqDTO>
    {
        private readonly ILogger<CreateBreakdownTaskHandler> _logger;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsByDtoAtomicHandler;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsByDtoAtomicHandler;
        private readonly GetBreakdownTasksByDtoAtomicHandler _getBreakdownTasksByDtoAtomicHandler;
        private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskAtomicHandler;

        public CreateBreakdownTaskHandler(
            ILogger<CreateBreakdownTaskHandler> logger,
            GetLocationsByDtoAtomicHandler getLocationsByDtoAtomicHandler,
            GetInstructionSetsByDtoAtomicHandler getInstructionSetsByDtoAtomicHandler,
            GetBreakdownTasksByDtoAtomicHandler getBreakdownTasksByDtoAtomicHandler,
            CreateBreakdownTaskAtomicHandler createBreakdownTaskAtomicHandler)
        {
            _logger = logger;
            _getLocationsByDtoAtomicHandler = getLocationsByDtoAtomicHandler;
            _getInstructionSetsByDtoAtomicHandler = getInstructionSetsByDtoAtomicHandler;
            _getBreakdownTasksByDtoAtomicHandler = getBreakdownTasksByDtoAtomicHandler;
            _createBreakdownTaskAtomicHandler = createBreakdownTaskAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateBreakdownTaskReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Breakdown Task");
            
            string? sessionId = RequestContext.GetSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BusinessCaseFailureException(
                    error: ErrorConstants.CAF_AUTHEN_0002,
                    errorDetails: ["SessionId not found in RequestContext"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
            }
            
            HttpResponseSnapshot getLocationsResponse = await GetLocationsFromDownstream(request, sessionId);
            if (!getLocationsResponse.IsSuccessStatusCode)
            {
                _logger.Error($"GetLocationsByDto failed: {getLocationsResponse.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)getLocationsResponse.StatusCode,
                    error: ErrorConstants.CAF_TSKCRT_0003,
                    errorDetails: [$"Failed to get locations. Status: {getLocationsResponse.StatusCode}. Response: {getLocationsResponse.Content}"],
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
            }
            else
            {
                GetLocationsByDtoApiResDTO? locationsApiResponse = SOAPHelper.DeserializeSoapResponse<GetLocationsByDtoApiResDTO>(getLocationsResponse.Content!);
                
                if (locationsApiResponse?.Envelope?.Body?.GetLocationsByDtoResponse?.GetLocationsByDtoResult?.Locations == null 
                    || !locationsApiResponse.Envelope.Body.GetLocationsByDtoResponse.GetLocationsByDtoResult.Locations.Any())
                {
                    throw new NotFoundException(
                        error: ErrorConstants.CAF_TSKCRT_0003,
                        errorDetails: [$"No locations found for Property: {request.PropertyName}, Unit: {request.UnitCode}"],
                        stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
                }
                else
                {
                    LocationItemDTO firstLocation = locationsApiResponse.Envelope.Body.GetLocationsByDtoResponse.GetLocationsByDtoResult.Locations.First();
                    int buildingId = firstLocation.BuildingId;
                    int locationId = firstLocation.LocationId;
                    
                    HttpResponseSnapshot getInstructionsResponse = await GetInstructionsFromDownstream(request, sessionId);
                    if (!getInstructionsResponse.IsSuccessStatusCode)
                    {
                        _logger.Error($"GetInstructionSetsByDto failed: {getInstructionsResponse.StatusCode}");
                        throw new DownStreamApiFailureException(
                            statusCode: (HttpStatusCode)getInstructionsResponse.StatusCode,
                            error: ErrorConstants.CAF_TSKCRT_0004,
                            errorDetails: [$"Failed to get instructions. Status: {getInstructionsResponse.StatusCode}. Response: {getInstructionsResponse.Content}"],
                            stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
                    }
                    else
                    {
                        GetInstructionSetsByDtoApiResDTO? instructionsApiResponse = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsByDtoApiResDTO>(getInstructionsResponse.Content!);
                        
                        int instructionId = instructionsApiResponse?.Envelope?.Body?.GetInstructionSetsByDtoResponse?.GetInstructionSetsByDtoResult?.Instructions?.FirstOrDefault()?.InstructionId ?? 0;
                        int categoryId = instructionsApiResponse?.Envelope?.Body?.GetInstructionSetsByDtoResponse?.GetInstructionSetsByDtoResult?.Instructions?.FirstOrDefault()?.CategoryId ?? 0;
                        
                        HttpResponseSnapshot createTaskResponse = await CreateBreakdownTaskInDownstream(request, sessionId, buildingId, locationId, categoryId, instructionId);
                        if (!createTaskResponse.IsSuccessStatusCode)
                        {
                            _logger.Error($"CreateBreakdownTask failed: {createTaskResponse.StatusCode}");
                            throw new DownStreamApiFailureException(
                                statusCode: (HttpStatusCode)createTaskResponse.StatusCode,
                                error: ErrorConstants.CAF_TSKCRT_0001,
                                errorDetails: [$"Failed to create breakdown task. Status: {createTaskResponse.StatusCode}. Response: {createTaskResponse.Content}"],
                                stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
                        }
                        else
                        {
                            CreateBreakdownTaskApiResDTO? createTaskApiResponse = SOAPHelper.DeserializeSoapResponse<CreateBreakdownTaskApiResDTO>(createTaskResponse.Content!);
                            
                            if (createTaskApiResponse?.Envelope?.Body?.CreateBreakdownTaskResponse?.CreateBreakdownTaskResult == null)
                            {
                                throw new NoResponseBodyException(
                                    error: ErrorConstants.CAF_TSKCRT_0002,
                                    errorDetails: ["CreateBreakdownTask returned empty result"],
                                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
                            }
                            else
                            {
                                _logger.Info("[System Layer]-Completed Create Breakdown Task");
                                return new BaseResponseDTO(
                                    message: InfoConstants.CREATE_BREAKDOWN_TASK_SUCCESS,
                                    data: CreateBreakdownTaskResDTO.Map(createTaskApiResponse),
                                    errorCode: null);
                            }
                        }
                    }
                }
            }
        }

        private async Task<HttpResponseSnapshot> GetLocationsFromDownstream(CreateBreakdownTaskReqDTO request, string sessionId)
        {
            GetLocationsByDtoHandlerReqDTO atomicRequest = new GetLocationsByDtoHandlerReqDTO
            {
                SessionId = sessionId,
                PropertyName = request.PropertyName,
                UnitCode = request.UnitCode
            };
            return await _getLocationsByDtoAtomicHandler.Handle(atomicRequest);
        }

        private async Task<HttpResponseSnapshot> GetInstructionsFromDownstream(CreateBreakdownTaskReqDTO request, string sessionId)
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
            int buildingId,
            int locationId,
            int categoryId,
            int instructionId)
        {
            CreateBreakdownTaskHandlerReqDTO atomicRequest = new CreateBreakdownTaskHandlerReqDTO
            {
                SessionId = sessionId,
                ReporterName = request.ReporterName,
                ReporterEmail = request.ReporterEmail,
                ReporterPhoneNumber = request.ReporterPhoneNumber,
                ServiceRequestNumber = request.ServiceRequestNumber,
                Description = request.Description,
                CategoryId = categoryId,
                DisciplineId = 0,
                PriorityId = 0,
                BuildingId = buildingId,
                LocationId = locationId,
                InstructionId = instructionId,
                ScheduledDateUtc = request.ScheduledDate,
                RaisedDateUtc = request.RaisedDateUtc,
                ContractId = 0,
                CallerSourceId = request.SourceOrgId
            };
            return await _createBreakdownTaskAtomicHandler.Handle(atomicRequest);
        }
    }
}

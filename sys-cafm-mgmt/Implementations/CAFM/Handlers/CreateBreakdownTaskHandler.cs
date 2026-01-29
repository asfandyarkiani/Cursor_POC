using CAFMSystem.Constants;
using CAFMSystem.DTO.AtomicHandlerDTOs;
using CAFMSystem.DTO.CreateBreakdownTaskDTO;
using CAFMSystem.DTO.DownstreamDTOs;
using CAFMSystem.Helper;
using CAFMSystem.Implementations.CAFM.AtomicHandlers;
using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Core.SystemLayer.Exceptions;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace CAFMSystem.Implementations.CAFM.Handlers
{
    public class CreateBreakdownTaskHandler : IBaseHandler<CreateBreakdownTaskReqDTO>
    {
        private readonly ILogger<CreateBreakdownTaskHandler> _logger;
        private readonly GetLocationsByDtoAtomicHandler _getLocationsByDtoAtomicHandler;
        private readonly GetInstructionSetsByDtoAtomicHandler _getInstructionSetsByDtoAtomicHandler;
        private readonly CreateBreakdownTaskAtomicHandler _createBreakdownTaskAtomicHandler;

        public CreateBreakdownTaskHandler(
            ILogger<CreateBreakdownTaskHandler> logger,
            GetLocationsByDtoAtomicHandler getLocationsByDtoAtomicHandler,
            GetInstructionSetsByDtoAtomicHandler getInstructionSetsByDtoAtomicHandler,
            CreateBreakdownTaskAtomicHandler createBreakdownTaskAtomicHandler)
        {
            _logger = logger;
            _getLocationsByDtoAtomicHandler = getLocationsByDtoAtomicHandler;
            _getInstructionSetsByDtoAtomicHandler = getInstructionSetsByDtoAtomicHandler;
            _createBreakdownTaskAtomicHandler = createBreakdownTaskAtomicHandler;
        }

        public async Task<BaseResponseDTO> HandleAsync(CreateBreakdownTaskReqDTO request)
        {
            _logger.Info("[System Layer]-Initiating Create Breakdown Task");
            
            string buildingId = string.Empty;
            string locationId = string.Empty;
            string categoryId = string.Empty;
            string disciplineId = string.Empty;
            string priorityId = string.Empty;
            string instructionId = string.Empty;
            
            HttpResponseSnapshot locationResponse = await GetLocationsByDtoFromDownstream(request);
            
            if (!locationResponse.IsSuccessStatusCode)
            {
                _logger.Warn($"GetLocationsByDto failed with status: {locationResponse.StatusCode} - Continuing with empty values");
                buildingId = string.Empty;
                locationId = string.Empty;
            }
            else
            {
                GetLocationsByDtoApiResDTO? locationApiResponse = SOAPHelper.DeserializeSoapResponse<GetLocationsByDtoApiResDTO>(locationResponse.Content!);
                buildingId = locationApiResponse?.BuildingId ?? string.Empty;
                locationId = locationApiResponse?.LocationId ?? string.Empty;
                _logger.Info($"Location lookup succeeded: BuildingId={buildingId}, LocationId={locationId}");
            }
            
            HttpResponseSnapshot instructionResponse = await GetInstructionSetsByDtoFromDownstream(request);
            
            if (!instructionResponse.IsSuccessStatusCode)
            {
                _logger.Warn($"GetInstructionSetsByDto failed with status: {instructionResponse.StatusCode} - Continuing with empty values");
                categoryId = string.Empty;
                disciplineId = string.Empty;
                priorityId = string.Empty;
                instructionId = string.Empty;
            }
            else
            {
                GetInstructionSetsByDtoApiResDTO? instructionApiResponse = SOAPHelper.DeserializeSoapResponse<GetInstructionSetsByDtoApiResDTO>(instructionResponse.Content!);
                categoryId = instructionApiResponse?.IN_FKEY_CAT_SEQ ?? string.Empty;
                disciplineId = instructionApiResponse?.IN_FKEY_LAB_SEQ ?? string.Empty;
                priorityId = instructionApiResponse?.IN_FKEY_PRI_SEQ ?? string.Empty;
                instructionId = instructionApiResponse?.IN_SEQ ?? string.Empty;
                _logger.Info($"Instruction sets lookup succeeded: CategoryId={categoryId}, DisciplineId={disciplineId}, PriorityId={priorityId}, InstructionId={instructionId}");
            }
            
            string scheduledDateUtc = FormatScheduledDateUtc(request.TicketDetails.ScheduledDate, request.TicketDetails.ScheduledTimeStart);
            string raisedDateUtc = FormatRaisedDateUtc(request.TicketDetails.RaisedDateUtc);
            
            HttpResponseSnapshot createResponse = await CreateBreakdownTaskInDownstream(
                request, 
                buildingId, 
                locationId, 
                categoryId, 
                disciplineId, 
                priorityId, 
                instructionId, 
                scheduledDateUtc, 
                raisedDateUtc);
            
            if (!createResponse.IsSuccessStatusCode)
            {
                _logger.Error($"CreateBreakdownTask failed with status: {createResponse.StatusCode}");
                throw new DownStreamApiFailureException(
                    statusCode: (HttpStatusCode)createResponse.StatusCode,
                    error: ErrorConstants.CAF_TSKCRT_0001,
                    errorDetails: new string[] { $"CAFM API failed. Status: {createResponse.StatusCode}. Response: {createResponse.Content}" },
                    stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
            }
            else
            {
                CreateBreakdownTaskApiResDTO? apiResponse = SOAPHelper.DeserializeSoapResponse<CreateBreakdownTaskApiResDTO>(createResponse.Content!);
                
                if (apiResponse == null || string.IsNullOrWhiteSpace(apiResponse.PrimaryKeyId))
                {
                    throw new NoResponseBodyException(
                        errorDetails: new string[] { "CAFM returned empty PrimaryKeyId for breakdown task creation" },
                        stepName: "CreateBreakdownTaskHandler.cs / HandleAsync");
                }
                else
                {
                    _logger.Info("[System Layer]-Completed Create Breakdown Task");
                    
                    return new BaseResponseDTO(
                        message: InfoConstants.CREATE_BREAKDOWN_TASK_SUCCESS,
                        data: CreateBreakdownTaskResDTO.Map(apiResponse, request.ServiceRequestNumber, request.SourceOrgId),
                        errorCode: null);
                }
            }
        }

        private async Task<HttpResponseSnapshot> GetLocationsByDtoFromDownstream(CreateBreakdownTaskReqDTO request)
        {
            GetLocationsByDtoHandlerReqDTO atomicRequest = new GetLocationsByDtoHandlerReqDTO
            {
                UnitCode = request.UnitCode
            };
            
            return await _getLocationsByDtoAtomicHandler.Handle(atomicRequest);
        }

        private async Task<HttpResponseSnapshot> GetInstructionSetsByDtoFromDownstream(CreateBreakdownTaskReqDTO request)
        {
            GetInstructionSetsByDtoHandlerReqDTO atomicRequest = new GetInstructionSetsByDtoHandlerReqDTO
            {
                SubCategory = request.SubCategory
            };
            
            return await _getInstructionSetsByDtoAtomicHandler.Handle(atomicRequest);
        }

        private async Task<HttpResponseSnapshot> CreateBreakdownTaskInDownstream(
            CreateBreakdownTaskReqDTO request,
            string buildingId,
            string locationId,
            string categoryId,
            string disciplineId,
            string priorityId,
            string instructionId,
            string scheduledDateUtc,
            string raisedDateUtc)
        {
            CreateBreakdownTaskHandlerReqDTO atomicRequest = new CreateBreakdownTaskHandlerReqDTO
            {
                ReporterName = request.ReporterName,
                ReporterEmail = request.ReporterEmail,
                ReporterPhoneNumber = request.ReporterPhoneNumber,
                ServiceRequestNumber = request.ServiceRequestNumber,
                Description = request.Description,
                CategoryId = categoryId,
                DisciplineId = disciplineId,
                PriorityId = priorityId,
                BuildingId = buildingId,
                LocationId = locationId,
                InstructionId = instructionId,
                ScheduledDateUtc = scheduledDateUtc,
                RaisedDateUtc = raisedDateUtc,
                ContractId = "1",
                SourceOrgId = request.SourceOrgId
            };
            
            return await _createBreakdownTaskAtomicHandler.Handle(atomicRequest);
        }

        private string FormatScheduledDateUtc(string scheduledDate, string scheduledTimeStart)
        {
            if (string.IsNullOrWhiteSpace(scheduledDate))
            {
                return string.Empty;
            }
            else
            {
                try
                {
                    DateTime date = DateTime.Parse(scheduledDate);
                    
                    if (!string.IsNullOrWhiteSpace(scheduledTimeStart))
                    {
                        TimeSpan time = TimeSpan.Parse(scheduledTimeStart);
                        date = date.Add(time);
                    }
                    else
                    {
                        _logger.Debug("ScheduledTimeStart is empty, using date only");
                    }
                    
                    return date.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Failed to parse scheduled date/time: {ex.Message} - Using empty value");
                    return string.Empty;
                }
            }
        }

        private string FormatRaisedDateUtc(string raisedDateUtc)
        {
            if (string.IsNullOrWhiteSpace(raisedDateUtc))
            {
                return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z", CultureInfo.InvariantCulture);
            }
            else
            {
                try
                {
                    DateTime date = DateTime.Parse(raisedDateUtc);
                    return date.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    _logger.Warn($"Failed to parse raised date: {ex.Message} - Using current UTC time");
                    return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.0208713Z", CultureInfo.InvariantCulture);
                }
            }
        }
    }
}

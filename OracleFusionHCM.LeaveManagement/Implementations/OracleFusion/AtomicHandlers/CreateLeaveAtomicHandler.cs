using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHCM.LeaveManagement.ConfigModels;
using OracleFusionHCM.LeaveManagement.Constants;
using OracleFusionHCM.LeaveManagement.DTO.AtomicHandlerDTOs;
using System.Text;
using System.Text.Json;

namespace OracleFusionHCM.LeaveManagement.Implementations.OracleFusion.AtomicHandlers
{
    /// <summary>
    /// Atomic Handler for creating leave entry in Oracle Fusion HCM
    /// Makes EXACTLY ONE external HTTP POST call to Oracle Fusion API
    /// Authentication: Basic Auth (credentials per request)
    /// </summary>
    public class CreateLeaveAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly CustomRestClient _restClient;
        private readonly AppConfigs _appConfigs;
        private readonly ILogger<CreateLeaveAtomicHandler> _logger;
        
        public CreateLeaveAtomicHandler(
            CustomRestClient restClient,
            IOptions<AppConfigs> options,
            ILogger<CreateLeaveAtomicHandler> logger)
        {
            _restClient = restClient;
            _appConfigs = options.Value;
            _logger = logger;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateLeaveHandlerReqDTO requestDTO = downStreamRequestDTO as CreateLeaveHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type - expected CreateLeaveHandlerReqDTO");
            
            _logger.Info($"Starting CreateLeave for PersonNumber: {requestDTO.PersonNumber}");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            string fullUrl = $"{_appConfigs.OracleFusionBaseUrl}/{_appConfigs.LeaveResourcePath}";
            
            // Build request body in Oracle Fusion format (based on map_c426b4d6)
            object requestBody = new
            {
                personNumber = requestDTO.PersonNumber,
                absenceType = requestDTO.AbsenceType,
                employer = requestDTO.Employer,
                startDate = requestDTO.StartDate,
                endDate = requestDTO.EndDate,
                absenceStatusCd = requestDTO.AbsenceStatusCd,
                approvalStatusCd = requestDTO.ApprovalStatusCd,
                startDateDuration = requestDTO.StartDateDuration,
                endDateDuration = requestDTO.EndDateDuration
            };
            
            // Create Basic Auth header
            string credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{requestDTO.Username}:{requestDTO.Password}")
            );
            
            Dictionary<string, string> customHeaders = new Dictionary<string, string>
            {
                { "Authorization", $"Basic {credentials}" },
                { "Content-Type", "application/json" }
            };
            
            HttpResponseSnapshot response = await _restClient.ExecuteCustomRestRequestAsync(
                operationName: OperationNames.CREATE_LEAVE,
                apiUrl: fullUrl,
                httpMethod: HttpMethod.Post,
                contentFactory: () => new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                ),
                username: null,
                password: null,
                queryParameters: null,
                customHeaders: customHeaders
            );
            
            _logger.Info($"CreateLeave completed - Status: {response.StatusCode}");
            
            return response;
        }
    }
}

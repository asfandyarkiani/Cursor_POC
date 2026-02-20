using Core.Extensions;
using Core.Middlewares;
using Core.SystemLayer.DTOs;
using Core.SystemLayer.Handlers;
using Core.SystemLayer.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OracleFusionHcmMgmt.ConfigModels;
using OracleFusionHcmMgmt.Constants;
using OracleFusionHcmMgmt.DTO.AtomicHandlerDTOs;
using System.Net;
using System.Text;

namespace OracleFusionHcmMgmt.Implementations.OracleFusion.AtomicHandlers
{
    /// <summary>
    /// Atomic Handler for creating leave absence in Oracle Fusion HCM.
    /// Makes EXACTLY ONE external HTTP POST call to Oracle Fusion HCM REST API.
    /// </summary>
    public class CreateLeaveAtomicHandler : IAtomicHandler<HttpResponseSnapshot>
    {
        private readonly ILogger<CreateLeaveAtomicHandler> _logger;
        private readonly CustomRestClient _customRestClient;
        private readonly AppConfigs _appConfigs;
        
        public CreateLeaveAtomicHandler(
            ILogger<CreateLeaveAtomicHandler> logger,
            CustomRestClient customRestClient,
            IOptions<AppConfigs> options)
        {
            _logger = logger;
            _customRestClient = customRestClient;
            _appConfigs = options.Value;
        }
        
        public async Task<HttpResponseSnapshot> Handle(IDownStreamRequestDTO downStreamRequestDTO)
        {
            CreateLeaveHandlerReqDTO requestDTO = downStreamRequestDTO as CreateLeaveHandlerReqDTO 
                ?? throw new ArgumentException("Invalid DTO type. Expected CreateLeaveHandlerReqDTO.");
            
            requestDTO.ValidateDownStreamRequestParameters();
            
            _logger.Info($"CreateLeaveAtomicHandler processing for PersonNumber: {requestDTO.PersonNumber}");
            
            string fullUrl = $"{_appConfigs.OracleFusionBaseUrl}/{_appConfigs.OracleFusionResourcePath}";
            
            object requestBody = MapDtoToRequestBody(requestDTO);
            
            string credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{requestDTO.Username}:{requestDTO.Password}")
            );
            
            Dictionary<string, string> customHeaders = new Dictionary<string, string>
            {
                { "Authorization", $"Basic {credentials}" }
            };
            
            HttpResponseSnapshot response = await _customRestClient.ExecuteCustomRestRequestAsync(
                operationName: OperationNames.CREATE_LEAVE,
                apiUrl: fullUrl,
                httpMethod: HttpMethod.Post,
                contentFactory: () => CustomRestClient.CreateJsonContent(requestBody),
                username: null,
                password: null,
                queryParameters: null,
                customHeaders: customHeaders
            );
            
            _logger.Info($"CreateLeaveAtomicHandler completed - Status: {response.StatusCode}");
            
            return response;
        }
        
        /// <summary>
        /// Maps DTO to Oracle Fusion HCM request body format.
        /// Field transformations: D365 names to Oracle Fusion names.
        /// </summary>
        private object MapDtoToRequestBody(CreateLeaveHandlerReqDTO dto)
        {
            return new
            {
                personNumber = dto.PersonNumber,
                absenceType = dto.AbsenceType,
                employer = dto.Employer,
                startDate = dto.StartDate,
                endDate = dto.EndDate,
                absenceStatusCd = dto.AbsenceStatusCd,
                approvalStatusCd = dto.ApprovalStatusCd,
                startDateDuration = dto.StartDateDuration,
                endDateDuration = dto.EndDateDuration
            };
        }
    }
}

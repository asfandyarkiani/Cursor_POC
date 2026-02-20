using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OracleFusionHcmSystemLayer.Abstractions;
using OracleFusionHcmSystemLayer.Constants;
using OracleFusionHcmSystemLayer.DTO.CreateAbsenceDTO;

namespace OracleFusionHcmSystemLayer.Functions
{
    public class CreateAbsenceAPI
    {
        private readonly ILogger<CreateAbsenceAPI> _logger;
        private readonly IAbsenceMgmt _absenceMgmt;

        public CreateAbsenceAPI(ILogger<CreateAbsenceAPI> logger, IAbsenceMgmt absenceMgmt)
        {
            _logger = logger;
            _absenceMgmt = absenceMgmt;
        }

        [Function("CreateAbsence")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hcm/absence/create")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Create Absence.");

            CreateAbsenceReqDTO? request = await req.ReadBodyAsync<CreateAbsenceReqDTO>();
            
            if (request == null)
            {
                _logger.Error("Request body is null or invalid.");
                throw new NoRequestBodyException(
                    errorDetails: [ErrorCodes.REQ_BODY_MISSING_OR_EMPTY.Message],
                    stepName: "CreateAbsenceAPI.cs / Executing Run"
                );
            }

            request.ValidateAPIRequestParameters();

            BaseResponseDTO result = await _absenceMgmt.CreateAbsence(request);
            
            return result;
        }
    }
}

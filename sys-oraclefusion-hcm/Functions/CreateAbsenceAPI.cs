using Core.DTOs;
using Core.Exceptions;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OracleFusionHCMSystemLayer.Abstractions;
using OracleFusionHCMSystemLayer.DTO.CreateAbsenceDTO;

namespace OracleFusionHCMSystemLayer.Functions
{
    public class CreateAbsenceAPI
    {
        private readonly ILogger<CreateAbsenceAPI> _logger;
        private readonly IAbsenceMgmt _absenceMgmt;

        public CreateAbsenceAPI(
            ILogger<CreateAbsenceAPI> logger,
            IAbsenceMgmt absenceMgmt)
        {
            _logger = logger;
            _absenceMgmt = absenceMgmt;
        }

        [Function("CreateAbsence")]
        public async Task<BaseResponseDTO> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "hcm/absences")] HttpRequest req,
            FunctionContext context)
        {
            _logger.Info("HTTP trigger received for Create Absence in Oracle Fusion HCM");

            CreateAbsenceReqDTO? request = await req.ReadBodyAsync<CreateAbsenceReqDTO>();

            if (request == null)
            {
                _logger.Error("Request body is null or invalid");
                throw new NoRequestBodyException(
                    errorDetails: ["Request body is missing or empty"],
                    stepName: "CreateAbsenceAPI.cs / Executing Run"
                );
            }

            request.ValidateAPIRequestParameters();

            BaseResponseDTO result = await _absenceMgmt.CreateAbsence(request);

            return result;
        }
    }
}

using Core.Extensions;
using Core.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcHcmLeave.ConfigModels;
using ProcHcmLeave.Domains;
using System.Dynamic;

namespace ProcHcmLeave.SystemAbstractions.HcmMgmt
{
    public interface IAbsenceMgmt
    {
        Task<HttpResponseMessage> CreateAbsence(Leave domain);
    }

    public class AbsenceMgmtSys : IAbsenceMgmt
    {
        private readonly AppConfigs _options;
        private readonly CustomHTTPClient _customHttpClient;
        private readonly ILogger<AbsenceMgmtSys> _logger;

        public AbsenceMgmtSys(IOptions<AppConfigs> options, CustomHTTPClient customHttpClient, ILogger<AbsenceMgmtSys> logger)
        {
            _options = options.Value;
            _customHttpClient = customHttpClient;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> CreateAbsence(Leave domain)
        {
            _logger.Info("[Process Layer]-Initiating CreateAbsence call to System Layer");

            dynamic request = new ExpandoObject();
            request.EmployeeNumber = domain.EmployeeNumber;
            request.AbsenceType = domain.AbsenceType;
            request.Employer = domain.Employer;
            request.StartDate = domain.StartDate;
            request.EndDate = domain.EndDate;
            request.AbsenceStatusCode = domain.AbsenceStatusCode;
            request.ApprovalStatusCode = domain.ApprovalStatusCode;
            request.StartDateDuration = domain.StartDateDuration;
            request.EndDateDuration = domain.EndDateDuration;

            string url = _options.CreateAbsenceUrl;

            HttpResponseMessage response = await _customHttpClient.SendProcessHTTPReqAsync(
                method: HttpMethod.Post,
                url: url,
                contentType: "application/json",
                body: (object)request,
                reqHeaders: null);

            _logger.Info("[Process Layer]-Completed CreateAbsence call to System Layer");
            
            return response;
        }
    }
}

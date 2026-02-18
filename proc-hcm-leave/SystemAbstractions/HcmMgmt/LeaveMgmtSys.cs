using Core.Middlewares;
using Core.Extensions;
using HcmLeaveProcessLayer.ConfigModels;
using HcmLeaveProcessLayer.Domains;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Dynamic;

namespace HcmLeaveProcessLayer.SystemAbstractions.HcmMgmt
{
    public interface ILeaveMgmt
    {
        Task<HttpResponseMessage> CreateLeave(Leave domain);
    }

    public class LeaveMgmtSys : ILeaveMgmt
    {
        private readonly AppConfigs _options;
        private readonly CustomHTTPClient _customHttpClient;
        private readonly ILogger<LeaveMgmtSys> _logger;

        public LeaveMgmtSys(IOptions<AppConfigs> options, CustomHTTPClient customHttpClient, ILogger<LeaveMgmtSys> logger)
        {
            _options = options.Value;
            _customHttpClient = customHttpClient;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> CreateLeave(Leave domain)
        {
            _logger.Info("[Process Layer]-Initiating CreateLeave System Layer call");

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

            string url = _options.CreateAbsenceSystemLayerUrl;

            HttpResponseMessage response = await _customHttpClient.SendProcessHTTPReqAsync(
                method: HttpMethod.Post,
                url: url,
                contentType: "application/json",
                body: (object)request,
                reqHeaders: null);

            _logger.Info("[Process Layer]-Completed CreateLeave System Layer call");

            return response;
        }
    }
}

using Core.Extensions;
using Core.Middlewares;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProcHcmLeave.ConfigModels;
using ProcHcmLeave.DTOs.CreateLeave;
using ProcHcmLeave.SystemAbstractions.HcmMgmt.Interfaces;
using System.Dynamic;

namespace ProcHcmLeave.SystemAbstractions.HcmMgmt
{
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

        public async Task<HttpResponseMessage> CreateLeave(CreateLeaveReqDTO request)
        {
            _logger.Info("[Process Layer]-Initiating CreateLeave System Layer call");

            dynamic dynamicReq = new ExpandoObject();
            dynamicReq.EmployeeNumber = request.EmployeeNumber;
            dynamicReq.AbsenceType = request.AbsenceType;
            dynamicReq.Employer = request.Employer;
            dynamicReq.StartDate = request.StartDate;
            dynamicReq.EndDate = request.EndDate;
            dynamicReq.AbsenceStatusCode = request.AbsenceStatusCode;
            dynamicReq.ApprovalStatusCode = request.ApprovalStatusCode;
            dynamicReq.StartDateDuration = request.StartDateDuration;
            dynamicReq.EndDateDuration = request.EndDateDuration;

            string url = _options.CreateAbsenceUrl;

            HttpResponseMessage response = await _customHttpClient.SendProcessHTTPReqAsync(
                method: HttpMethod.Post,
                url: url,
                contentType: "application/json",
                body: (object)dynamicReq,
                reqHeaders: null);

            _logger.Info("[Process Layer]-Completed CreateLeave System Layer call");

            return response;
        }
    }
}

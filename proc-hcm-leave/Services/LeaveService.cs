using Core.Extensions;
using Core.Middlewares;
using HcmLeaveProcessLayer.Domains;
using HcmLeaveProcessLayer.SystemAbstractions.HcmMgmt;
using Microsoft.Extensions.Logging;

namespace HcmLeaveProcessLayer.Services
{
    public class LeaveService
    {
        private readonly ILogger<LeaveService> _logger;
        private readonly ILeaveMgmt _leaveMgmt;

        public LeaveService(ILogger<LeaveService> logger, ILeaveMgmt leaveMgmt)
        {
            _logger = logger;
            _leaveMgmt = leaveMgmt;
        }

        public async Task<HttpResponseMessage> CreateLeave(Leave domain)
        {
            _logger.Info("[Process Layer]-Initiating CreateLeave service call");

            HttpResponseMessage response = await _leaveMgmt.CreateLeave(domain);

            _logger.Info("[Process Layer]-Completed CreateLeave service call");

            return response;
        }
    }
}

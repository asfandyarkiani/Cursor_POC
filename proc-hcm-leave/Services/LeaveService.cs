using Core.Extensions;
using Core.Snapshots;
using Microsoft.Extensions.Logging;
using ProcHcmLeave.DTOs.CreateLeave;
using ProcHcmLeave.SystemAbstractions.HcmMgmt.Interfaces;

namespace ProcHcmLeave.Services
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

        public async Task<HttpResponseMessage> CreateLeave(CreateLeaveReqDTO dto)
        {
            _logger.Info("[Process Layer]-Initiating CreateLeave");
            
            HttpResponseMessage response = await _leaveMgmt.CreateLeave(dto);
            
            _logger.Info("[Process Layer]-Completed CreateLeave");
            
            return response;
        }
    }
}

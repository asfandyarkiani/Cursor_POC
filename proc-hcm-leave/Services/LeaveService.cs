using Core.DTOs;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using ProcHcmLeave.Domains;
using ProcHcmLeave.SystemAbstractions.HcmMgmt;

namespace ProcHcmLeave.Services
{
    public class LeaveService
    {
        private readonly ILogger<LeaveService> _logger;
        private readonly IAbsenceMgmt _absenceMgmt;

        public LeaveService(ILogger<LeaveService> logger, IAbsenceMgmt absenceMgmt)
        {
            _logger = logger;
            _absenceMgmt = absenceMgmt;
        }

        public async Task<HttpResponseMessage> CreateLeave(Leave domain)
        {
            _logger.Info("[Process Layer]-Initiating CreateLeave");
            
            HttpResponseMessage response = await _absenceMgmt.CreateAbsence(domain);
            
            _logger.Info("[Process Layer]-Completed CreateLeave");
            
            return response;
        }
    }
}

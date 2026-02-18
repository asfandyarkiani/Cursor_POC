using Core.Extensions;
using ProcHcmLeave.DTOs.CreateLeave;
using System.Text.Json;

namespace ProcHcmLeave.Helper
{
    public static class ResponseDTOHelper
    {
        public static void PopulateCreateLeaveRes(string json, CreateLeaveResDTO dto)
        {
            Dictionary<string, object>? docDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            if (docDict != null)
            {
                dto.PersonAbsenceEntryId = docDict.ToLongValue("PersonAbsenceEntryId");
                dto.Status = docDict.ToStringValue("Status");
                dto.Message = docDict.ToStringValue("Message");
                dto.Success = docDict.ToStringValue("Success");
            }
        }
    }
}

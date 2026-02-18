using Core.Extensions;
using System.Text.Json;

namespace HcmLeaveProcessLayer.Helper
{
    public static class ResponseDTOHelper
    {
        public static void PopulateCreateLeaveRes(string json, DTOs.CreateLeave.CreateLeaveResDTO dto)
        {
            Dictionary<string, object>? docDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (docDict != null)
            {
                dto.PersonAbsenceEntryId = docDict.ToLongValue("PersonAbsenceEntryId");
                
                dto.Status = "success";
                dto.Message = "Data successfully sent to Oracle Fusion";
                dto.Success = "true";
            }
        }
    }
}

namespace ProcHcmLeave.ConfigModels
{
    public class AppConfigs
    {
        public static string SectionName = "AppVariables";
        
        public string ASPNETCORE_ENVIRONMENT { get; set; } = string.Empty;
        
        // System Layer URLs
        public string CreateAbsenceUrl { get; set; } = string.Empty;
        
        // Error notification configuration
        public string ErrorNotificationToEmail { get; set; } = string.Empty;
        public string ErrorNotificationFromEmail { get; set; } = string.Empty;
        public string ErrorNotificationSubjectPrefix { get; set; } = string.Empty;
        
        // Operation-specific error email configuration
        public string CreateLeaveErrorEmailFileNamePrefix { get; set; } = string.Empty;
        public string CreateLeaveErrorEmailHasAttachment { get; set; } = string.Empty;
    }
}

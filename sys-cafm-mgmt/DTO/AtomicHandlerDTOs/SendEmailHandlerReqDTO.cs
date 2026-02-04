using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace sys_cafm_mgmt.DTO.AtomicHandlerDTOs
{
    public class SendEmailHandlerReqDTO : IDownStreamRequestDTO
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContent { get; set; }

        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(To))
                errors.Add("To email address is required");
            
            if (string.IsNullOrWhiteSpace(Subject))
                errors.Add("Subject is required");
            
            if (string.IsNullOrWhiteSpace(Body))
                errors.Add("Body is required");
            
            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "SendEmailHandlerReqDTO.cs / Executing ValidateDownStreamRequestParameters");
        }
    }
}

using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace FsiCafmSystem.DTO.AtomicHandlerDTOs
{
    public class SendEmailHandlerReqDTO : IDownStreamRequestDTO
    {
        public string FromAddress { get; set; } = string.Empty;
        public string ToAddress { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool HasAttachment { get; set; }
        public string? AttachmentContent { get; set; }
        public string? AttachmentFileName { get; set; }
        
        public void ValidateDownStreamRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(FromAddress))
            {
                errors.Add("FromAddress is required.");
            }
            
            if (string.IsNullOrWhiteSpace(ToAddress))
            {
                errors.Add("ToAddress is required.");
            }
            
            if (string.IsNullOrWhiteSpace(Subject))
            {
                errors.Add("Subject is required.");
            }
            
            if (string.IsNullOrWhiteSpace(Body))
            {
                errors.Add("Body is required.");
            }
            
            if (errors.Count > 0)
            {
                throw new RequestValidationFailureException(
                    error: ("EML_VAL_0001", "Validation failed"),
                    errorDetails: errors,
                    stepName: "SendEmailHandlerReqDTO / ValidateDownStreamRequestParameters");
            }
        }
    }
}

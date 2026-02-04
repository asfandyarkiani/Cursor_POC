using Core.Exceptions;
using Core.SystemLayer.DTOs;

namespace FsiCafmSystem.DTO.HandlerDTOs.SendEmailDTO
{
    public class SendEmailReqDTO : IRequestSysDTO
    {
        public string ToAddress { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool HasAttachment { get; set; }
        public string? AttachmentContent { get; set; }
        public string? AttachmentFileName { get; set; }
        
        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(ToAddress))
            {
                errors.Add("ToAddress is required.");
            }
            else if (!ToAddress.Contains("@"))
            {
                errors.Add("ToAddress must be a valid email address.");
            }
            
            if (string.IsNullOrWhiteSpace(Subject))
            {
                errors.Add("Subject is required.");
            }
            
            if (string.IsNullOrWhiteSpace(Body))
            {
                errors.Add("Body is required.");
            }
            
            if (HasAttachment)
            {
                if (string.IsNullOrWhiteSpace(AttachmentContent))
                {
                    errors.Add("AttachmentContent is required when HasAttachment is true.");
                }
                
                if (string.IsNullOrWhiteSpace(AttachmentFileName))
                {
                    errors.Add("AttachmentFileName is required when HasAttachment is true.");
                }
            }
            
            if (errors.Any())
            {
                throw new RequestValidationFailureException
                {
                    ErrorProperties = errors
                };
            }
        }
    }
}

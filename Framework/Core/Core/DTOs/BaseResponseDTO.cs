
using Core.Models;

namespace Core.DTOs
{
    public class BaseResponseDTO
    {
        public BaseResponseDTO(string message, string errorCode, object? data, ErrorDetails? errorDetails = null, bool isDownStreamError = false, bool isPartialSuccess = false)
        {
            Message = message;
            ErrorCode = errorCode;
            Data = data;
            ErrorDetails = errorDetails;
            IsDownStreamError = isDownStreamError;
            IsPartialSuccess = isPartialSuccess;
        }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public object? Data { get; set; }
        public ErrorDetails? ErrorDetails { get; set; }
        public bool IsDownStreamError { get; set; }
        public bool IsPartialSuccess { get; set; }
    }
}

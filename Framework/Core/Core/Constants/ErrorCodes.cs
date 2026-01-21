namespace Core.Constants
{
    public class ErrorCodes
    {
        public static readonly (string ErrorCode, string Message) APP_UNHANDLED_INTERNAL_ERROR =
            ("COR_GENRIC_0001", "An unexpected error occurred.");

        public static readonly (string ErrorCode, string Message) DOWNSTREAM_API_FAILED =
            ("COR_GENRIC_0002", "A downstream API call failed while communicating with an external system.");

        public static readonly (string ErrorCode, string Message) REQ_BODY_MISSING_OR_EMPTY =
            ("COR_GENRIC_0003", "The HTTP request body is missing, empty, or not provided by the client.");

        public static readonly (string ErrorCode, string Message) REQ_VALIDATION_FAILURE =
            ("COR_GENRIC_0004", "Invalid request received.");

        public static readonly (string ErrorCode, string Message) BUSINESSCASE_FAILURE =
            ("COR_GENRIC_0005", "A business rule or condition was violated during request processing.");

        public static readonly (string ErrorCode, string Message) RES_BODY_MISSING_OR_EMPTY =
           ("COR_GENRIC_0006", "The HTTP response body is missing, empty, or not provided by the source.");

        public static readonly (string ErrorCode, string Message) RECORD_NOT_FOUND =
           ("COR_GENRIC_0007", "The requested record or resource was not found in the system.");

        public static readonly (string ErrorCode, string Message) HTTP_MISSING_API_URL =
            ("COR_GENRIC_0008", "The API URL is missing or not configured properly.");
    }
}

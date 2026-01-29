namespace FacilitiesMgmtSystem.Core.Exceptions;

/// <summary>
/// General API exception for API-related errors.
/// </summary>
public class ApiException : BaseException
{
    public int StatusCode { get; set; } = 500;

    public ApiException() : base("An API error occurred.")
    {
    }

    public ApiException(string message) : base(message)
    {
    }

    public ApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

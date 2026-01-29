using FacilitiesMgmtSystem.Core.Exceptions;

namespace FacilitiesMgmtSystem.Core.System.Exceptions;

/// <summary>
/// General exception for API errors.
/// </summary>
public class ApiException : BaseException
{
    public int? StatusCode { get; set; }

    public ApiException() : base() { }

    public ApiException(string message) : base(message) { }

    public ApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(string message, Exception innerException) 
        : base(message, innerException) { }
}

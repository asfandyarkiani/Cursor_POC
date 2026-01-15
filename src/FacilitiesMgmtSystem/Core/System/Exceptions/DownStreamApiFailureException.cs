using FacilitiesMgmtSystem.Core.Exceptions;

namespace FacilitiesMgmtSystem.Core.System.Exceptions;

/// <summary>
/// Exception thrown when a downstream API call fails.
/// </summary>
public class DownStreamApiFailureException : BaseException
{
    public int? StatusCode { get; set; }
    public string? ResponseBody { get; set; }

    public DownStreamApiFailureException() : base() { }

    public DownStreamApiFailureException(string message) : base(message) { }

    public DownStreamApiFailureException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public DownStreamApiFailureException(string message, Exception innerException) 
        : base(message, innerException) { }
}

namespace FacilitiesMgmtSystem.Core.Exceptions;

/// <summary>
/// Exception thrown when a downstream API call fails.
/// </summary>
public class DownStreamApiFailureException : BaseException
{
    public int? StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? DownstreamService { get; set; }

    public DownStreamApiFailureException() : base("Downstream API call failed.")
    {
    }

    public DownStreamApiFailureException(string message) : base(message)
    {
    }

    public DownStreamApiFailureException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public DownStreamApiFailureException(string message, int statusCode, string? responseBody = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

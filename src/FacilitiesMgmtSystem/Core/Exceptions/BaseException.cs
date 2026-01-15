namespace FacilitiesMgmtSystem.Core.Exceptions;

/// <summary>
/// Base exception for all application-specific exceptions.
/// </summary>
public class BaseException : Exception
{
    public string[] ErrorProperties { get; set; } = Array.Empty<string>();

    public BaseException() : base() { }

    public BaseException(string message) : base(message) { }

    public BaseException(string message, Exception innerException) 
        : base(message, innerException) { }
}

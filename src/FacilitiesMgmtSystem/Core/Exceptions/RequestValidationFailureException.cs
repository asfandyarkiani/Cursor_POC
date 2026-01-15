namespace FacilitiesMgmtSystem.Core.Exceptions;

/// <summary>
/// Exception thrown when request validation fails.
/// </summary>
public class RequestValidationFailureException : BaseException
{
    public RequestValidationFailureException() : base() { }

    public RequestValidationFailureException(string message) : base(message) { }

    public RequestValidationFailureException(string message, Exception innerException) 
        : base(message, innerException) { }
}

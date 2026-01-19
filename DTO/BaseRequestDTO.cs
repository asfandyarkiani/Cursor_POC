using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;

namespace FacilitiesMgmtSystem.DTO;

/// <summary>
/// Base request DTO with common properties and validation.
/// </summary>
public abstract class BaseRequestDTO
{
    /// <summary>
    /// MRI session ID populated by the middleware.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Validates the API request parameters.
    /// Override in derived classes to add specific validation.
    /// </summary>
    public virtual void ValidateAPIRequestParameters()
    {
        // Base validation - derived classes can override
    }

    /// <summary>
    /// Helper method to validate that a required field is not null or empty.
    /// </summary>
    protected void ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RequestValidationFailureException()
            {
                ErrorProperties = [string.Format(ErrorConstants.MISSING_REQUIRED_FIELD, fieldName)]
            };
        }
    }

    /// <summary>
    /// Helper method to validate that a required field is not null.
    /// </summary>
    protected void ValidateRequired<T>(T? value, string fieldName) where T : struct
    {
        if (!value.HasValue)
        {
            throw new RequestValidationFailureException()
            {
                ErrorProperties = [string.Format(ErrorConstants.MISSING_REQUIRED_FIELD, fieldName)]
            };
        }
    }
}

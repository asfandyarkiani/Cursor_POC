using Core.SystemLayer.DTOs;

namespace sys_cafm_workorder.DTOs.Api.CAFM;

/// <summary>
/// Request DTO for getting instruction sets from CAFM.
/// </summary>
public class GetInstructionSetsRequestDto : IRequestSysDTO
{
    /// <summary>
    /// CAFM Session ID for authentication.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Category name to search for instruction sets.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Sub-category name within the category.
    /// </summary>
    public string SubCategory { get; set; } = string.Empty;

    public void ValidateAPIRequestParameters()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SessionId))
            errors.Add("SessionId is required.");

        if (string.IsNullOrWhiteSpace(CategoryName))
            errors.Add("CategoryName is required.");

        if (errors.Count > 0)
        {
            throw new Core.Exceptions.RequestValidationFailureException(
                errorDetails: errors,
                stepName: nameof(GetInstructionSetsRequestDto));
        }
    }
}

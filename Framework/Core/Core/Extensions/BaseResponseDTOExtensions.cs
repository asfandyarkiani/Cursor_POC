using Core.DTOs;

namespace Core.Extensions
{
    public static class BaseResponseDTOExtensions
    {
        public static string? GetStepName(this BaseResponseDTO response)
        {
            return response?.ErrorDetails?.Errors?
                .FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.StepName))?
                .StepName;
        }

        public static List<string>? GetErrors(this BaseResponseDTO response)
        {
            return response?.ErrorDetails?.Errors?
                .Where(e => !string.IsNullOrWhiteSpace(e.StepError))
                .Select(e => e.StepError.Trim())
                .ToList()
                ?? new List<string>();
        }
    }
}

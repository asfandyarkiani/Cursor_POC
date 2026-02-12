using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Extension methods for HttpRequest.
/// </summary>
public static class HttpRequestExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Reads the request body and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="request">The HTTP request.</param>
    /// <returns>The deserialized object or null if the body is empty or invalid.</returns>
    public static async Task<T?> ReadBodyAsync<T>(this HttpRequest request) where T : class
    {
        try
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

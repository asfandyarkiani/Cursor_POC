using System.Text;
using System.Text.Json;
using Core.SystemLayer.Middlewares;

namespace sys_oraclefusionhcm_mgmt.Helpers;

/// <summary>
/// Helper class for REST API operations (JSON serialization/deserialization)
/// </summary>
public static class RestApiHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
    
    /// <summary>
    /// Serializes an object to JSON string
    /// </summary>
    public static string SerializeToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, JsonOptions);
    }
    
    /// <summary>
    /// Deserializes JSON string to object
    /// </summary>
    public static T? DeserializeJsonResponse<T>(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return default;
        }
        
        return JsonSerializer.Deserialize<T>(jsonContent, JsonOptions);
    }
    
    /// <summary>
    /// Creates StringContent for JSON payload
    /// </summary>
    public static StringContent CreateJsonContent<T>(T payload)
    {
        string jsonPayload = SerializeToJson(payload);
        return new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    }
    
    /// <summary>
    /// Validates HTTP response and extracts content
    /// </summary>
    public static async Task<string> ValidateAndExtractContentAsync(HttpResponseSnapshot responseSnapshot)
    {
        if (responseSnapshot == null)
        {
            throw new ArgumentNullException(nameof(responseSnapshot), "Response snapshot cannot be null");
        }
        
        if (string.IsNullOrWhiteSpace(responseSnapshot.Content))
        {
            throw new InvalidOperationException("Response content is empty or null");
        }
        
        return await Task.FromResult(responseSnapshot.Content);
    }
}

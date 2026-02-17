using System.Text;
using System.Text.Json;
using Core.SystemLayer.Middlewares;
using AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Constants;

namespace AGI.Enterprise.Automotive.BuddyApp.NotificationService.Mgmt.Helpers;

/// <summary>
/// Helper class for REST API operations
/// </summary>
public static class RestApiHelper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an object to JSON string
    /// </summary>
    public static string SerializeToJson<T>(T obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj), ErrorConstants.SYS_NTFSVC_1001_MSG);
        }

        return JsonSerializer.Serialize(obj, _jsonOptions);
    }

    /// <summary>
    /// Deserializes JSON string to object
    /// </summary>
    public static T DeserializeJsonResponse<T>(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            throw new ArgumentException(ErrorConstants.SYS_NTFSVC_1001_MSG, nameof(jsonContent));
        }

        T? deserializedObject = JsonSerializer.Deserialize<T>(jsonContent, _jsonOptions);
        
        if (deserializedObject == null)
        {
            throw new InvalidOperationException(ErrorConstants.SYS_NTFSVC_2005_MSG);
        }

        return deserializedObject;
    }

    /// <summary>
    /// Creates StringContent for HTTP request body
    /// </summary>
    public static StringContent CreateJsonContent<T>(T obj)
    {
        string jsonString = SerializeToJson(obj);
        return new StringContent(jsonString, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Extracts error message from HttpResponseSnapshot
    /// </summary>
    public static string ExtractErrorMessage(HttpResponseSnapshot httpResponseSnapshot)
    {
        if (httpResponseSnapshot == null)
        {
            return InfoConstants.DEFAULT_ERROR_MESSAGE;
        }

        if (!string.IsNullOrWhiteSpace(httpResponseSnapshot.Content))
        {
            return httpResponseSnapshot.Content;
        }

        if (!string.IsNullOrWhiteSpace(httpResponseSnapshot.ReasonPhrase))
        {
            return httpResponseSnapshot.ReasonPhrase;
        }

        return InfoConstants.DEFAULT_ERROR_MESSAGE;
    }

    /// <summary>
    /// Checks if HTTP status code is success (2xx)
    /// </summary>
    public static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
    }

    /// <summary>
    /// Checks if HTTP status code is client error (4xx)
    /// </summary>
    public static bool IsClientErrorStatusCode(int statusCode)
    {
        return statusCode >= 400 && statusCode < 500;
    }

    /// <summary>
    /// Checks if HTTP status code is server error (5xx)
    /// </summary>
    public static bool IsServerErrorStatusCode(int statusCode)
    {
        return statusCode >= 500 && statusCode < 600;
    }
}

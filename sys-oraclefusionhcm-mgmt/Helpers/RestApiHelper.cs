using Newtonsoft.Json;
using System.Text;

namespace AlGhurair.SystemLayer.OracleFusionHCM.Helpers;

/// <summary>
/// Helper class for REST API operations
/// </summary>
public static class RestApiHelper
{
    /// <summary>
    /// Serializes an object to JSON string
    /// </summary>
    public static string SerializeToJson<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
    }

    /// <summary>
    /// Deserializes JSON string to object
    /// </summary>
    public static T DeserializeJsonResponse<T>(string jsonContent)
    {
        T? deserializedResponse = JsonConvert.DeserializeObject<T>(jsonContent);
        
        if (deserializedResponse == null)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON response to type {typeof(T).Name}");
        }

        return deserializedResponse;
    }

    /// <summary>
    /// Creates StringContent for JSON payload
    /// </summary>
    public static StringContent CreateJsonContent(string jsonPayload)
    {
        return new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Creates Basic Authentication header value
    /// </summary>
    public static string CreateBasicAuthHeader(string username, string password)
    {
        string credentials = $"{username}:{password}";
        byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
        string base64Credentials = Convert.ToBase64String(credentialsBytes);
        return $"Basic {base64Credentials}";
    }
}

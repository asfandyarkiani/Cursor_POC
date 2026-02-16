using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace OracleFusionHcmSystemLayer.Helper
{
    public static class RestApiHelper
    {
        public static T? DeserializeJsonResponse<T>(string jsonContent)
        {
            ILogger<T> logger = ServiceLocator.GetRequiredService<ILogger<T>>();
            logger.Info($"Deserializing JSON to {typeof(T).Name}");
            
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            return JsonSerializer.Deserialize<T>(jsonContent, options);
        }

        public static string BuildUrl(string baseUrl, List<string> pathSegments)
        {
            ILogger logger = ServiceLocator.GetRequiredService<ILogger<RestApiHelper>>();
            logger.Info("Building URL from base and path segments");
            
            string url = baseUrl.TrimEnd('/');
            foreach (string segment in pathSegments)
            {
                url += "/" + segment.TrimStart('/');
            }
            
            return url;
        }
    }
}

using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace OracleFusionHCMSystemLayer.Helper
{
    public static class RestApiHelper
    {
        public static T? DeserializeJsonResponse<T>(string jsonContent)
        {
            ILogger<RestApiHelper> logger = ServiceLocator.GetRequiredService<ILogger<RestApiHelper>>();
            logger.Info($"Deserializing JSON to {typeof(T).Name}");

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(jsonContent, options);
        }

        public static string BuildUrl(string baseUrl, List<string> pathSegments)
        {
            ILogger<RestApiHelper> logger = ServiceLocator.GetRequiredService<ILogger<RestApiHelper>>();

            string url = baseUrl.TrimEnd('/');
            foreach (string segment in pathSegments)
            {
                url += "/" + segment.TrimStart('/');
            }

            logger.Info($"Built URL: {url}");
            return url;
        }

        public static string BuildUrl(string baseUrl, string resourcePath)
        {
            ILogger<RestApiHelper> logger = ServiceLocator.GetRequiredService<ILogger<RestApiHelper>>();

            string url = baseUrl.TrimEnd('/') + "/" + resourcePath.TrimStart('/');

            logger.Info($"Built URL: {url}");
            return url;
        }
    }
}

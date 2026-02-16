using Core.DI;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace sys_oraclefusion_hcm.Helper
{
    public static class RestApiHelper
    {
        public static T? DeserializeJsonResponse<T>(string jsonContent)
        {
            ILogger<T> logger = ServiceLocator.GetRequiredService<ILogger<T>>();
            logger.Info($"Deserializing JSON to {typeof(T).Name}");

            return JsonSerializer.Deserialize<T>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}

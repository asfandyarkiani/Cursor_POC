using Core.DTOs;
using Core.SystemLayer.Middlewares;
using System.Text.Json;

namespace Core.Extensions
{
    public static class HttpResponseSnapshotExtensions
    {
        public static string ExtractData(this HttpResponseSnapshot response)
        {
            String res = response.Content;
            using JsonDocument doc = JsonDocument.Parse(res);
            return doc.RootElement.GetProperty("Data").GetRawText();
        }

        public static BaseResponseDTO ExtractBaseResponse(this HttpResponseSnapshot response)
        {
            string errorContent = response.Content;
            BaseResponseDTO errorResponse = JsonSerializer.Deserialize<BaseResponseDTO>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            return errorResponse;
        }
    }
}

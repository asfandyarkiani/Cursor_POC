using System.Text.Json.Serialization;

namespace Core.SystemLayer.Middlewares
{
    public class HttpResponseSnapshot
    {
        [JsonIgnore]
        public HttpResponseMessage? orignalHttpResponse { get; set; }
        public int StatusCode { get; set; }

        public string? ReasonPhrase { get; set; }

        public string? Version { get; set; }

        public bool IsSuccessStatusCode { get; set; }

        public Dictionary<string, List<string>> Headers { get; set; } = new();

        public Dictionary<string, List<string>> ContentHeaders { get; set; } = new();

        public string? Content { get; set; }

        public byte[]? ByteContent { get; set; }

        public Dictionary<string, List<string>>? TrailingHeaders { get; set; }

        public static async Task<HttpResponseSnapshot> FromAsync(HttpResponseMessage response)
        {
            return new HttpResponseSnapshot
            {
                orignalHttpResponse = response,
                StatusCode = (int)response.StatusCode,
                ReasonPhrase = response.ReasonPhrase,
                Version = response.Version.ToString(),
                IsSuccessStatusCode = response.IsSuccessStatusCode,
                Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToList()),
                ContentHeaders = response.Content.Headers.ToDictionary(h => h.Key, h => h.Value.ToList()),
                Content = response.Content != null ? await response.Content.ReadAsStringAsync() : null,
                ByteContent = response.Content != null ? await response.Content.ReadAsByteArrayAsync() : null
            };
        }
    }
}

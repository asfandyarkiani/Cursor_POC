using Core.DTOs;
using System.Text.Json;

namespace Core.Extensions
{
    public static class HttpResponseMessageExtensions
    {

        /// <summary>
        /// Extracts the value of the "Data" property from the HTTP response JSON body.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> to read from.</param>
        /// <returns>
        /// A JSON string representing the raw value of the "Data" property.
        /// </returns>
        public static async Task<string> ExtractDataAsync(this HttpResponseMessage response)
        {
            using Stream stream = await response.Content.ReadAsStreamAsync();
            using StreamReader reader = new StreamReader(stream);
            String res = await reader.ReadToEndAsync();
            using JsonDocument doc = JsonDocument.Parse(res);
            return doc.RootElement.GetProperty("Data").GetRawText();
        }

        /// <summary>
        /// Extracts the entire response content as a raw string (full JSON body).
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> to read from.</param>
        /// <returns>
        /// A string containing the full response body.
        /// </returns>
        public static async Task<string> ExtractFullResponseAsync(this HttpResponseMessage response)
        {
            using Stream stream = await response.Content.ReadAsStreamAsync();
            using StreamReader reader = new StreamReader(stream);
            string res = await reader.ReadToEndAsync();
            return res;
        }

        /// <summary>
        /// Extracts the response content and deserializes it into a <see cref="BaseResponseDTO"/>.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> to read from.</param>
        /// <returns>
        /// A <see cref="BaseResponseDTO"/> object representing the response body.
        /// </returns>
        public static async Task<BaseResponseDTO> ExtractBaseResponse(this HttpResponseMessage response)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            BaseResponseDTO errorResponse = JsonSerializer.Deserialize<BaseResponseDTO>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            return errorResponse;
        }
    }
}

using System.Net;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Represents a snapshot of an HTTP response for SOAP calls.
/// </summary>
public class HttpResponseSnapshot
{
    public HttpStatusCode StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode < 300;
    public Dictionary<string, string> Headers { get; set; } = new();
}

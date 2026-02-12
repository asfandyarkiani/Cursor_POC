using System.Xml.Linq;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream response DTO for CAFM CreateBreakdownTask SOAP call.
/// </summary>
public class CAFMCreateBreakdownTaskDownstreamResponseDto
{
    /// <summary>
    /// The Task ID created in CAFM.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the task was created successfully.
    /// </summary>
    public bool IsSuccess => !string.IsNullOrWhiteSpace(TaskId);

    /// <summary>
    /// Raw XML response for debugging.
    /// </summary>
    public string RawResponse { get; set; } = string.Empty;

    /// <summary>
    /// Parses the SOAP XML response to extract the TaskId.
    /// </summary>
    public static CAFMCreateBreakdownTaskDownstreamResponseDto FromSoapXml(string xml)
    {
        var response = new CAFMCreateBreakdownTaskDownstreamResponseDto { RawResponse = xml };

        if (string.IsNullOrWhiteSpace(xml))
            return response;

        try
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

            // Try to find TaskId in the response
            var taskIdElement = doc.Descendants(ns + "TaskId").FirstOrDefault()
                ?? doc.Descendants("TaskId").FirstOrDefault();

            if (taskIdElement != null)
            {
                response.TaskId = taskIdElement.Value;
            }

            // Also check for CreateBreakdownTaskResult
            if (string.IsNullOrWhiteSpace(response.TaskId))
            {
                var resultElement = doc.Descendants(ns + "CreateBreakdownTaskResult").FirstOrDefault()
                    ?? doc.Descendants("CreateBreakdownTaskResult").FirstOrDefault();

                if (resultElement != null)
                {
                    var innerTaskId = resultElement.Descendants(ns + "TaskId").FirstOrDefault()
                        ?? resultElement.Descendants("TaskId").FirstOrDefault();
                    
                    if (innerTaskId != null)
                    {
                        response.TaskId = innerTaskId.Value;
                    }
                }
            }
        }
        catch
        {
            // If parsing fails, return empty response
        }

        return response;
    }
}

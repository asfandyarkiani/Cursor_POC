using System.Xml.Linq;
using sys_cafm_workorder.DTOs.Api.CAFM;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream response DTO for CAFM GetBreakdownTasksByDto SOAP call.
/// </summary>
public class CAFMGetBreakdownTasksDownstreamResponseDto
{
    /// <summary>
    /// List of breakdown tasks parsed from the response.
    /// </summary>
    public List<BreakdownTaskDto> Tasks { get; set; } = new();

    /// <summary>
    /// Raw XML response for debugging.
    /// </summary>
    public string RawResponse { get; set; } = string.Empty;

    /// <summary>
    /// Parses the SOAP XML response to extract breakdown tasks.
    /// </summary>
    public static CAFMGetBreakdownTasksDownstreamResponseDto FromSoapXml(string xml)
    {
        var response = new CAFMGetBreakdownTasksDownstreamResponseDto { RawResponse = xml };

        if (string.IsNullOrWhiteSpace(xml))
            return response;

        try
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

            var taskElements = doc.Descendants(ns + "BreakdownTaskDto")
                .Concat(doc.Descendants("BreakdownTaskDto"));

            foreach (var taskElement in taskElements)
            {
                var task = new BreakdownTaskDto
                {
                    TaskId = GetElementValue(taskElement, ns, "TaskId"),
                    CallId = GetElementValue(taskElement, ns, "CallId"),
                    BuildingId = GetElementValue(taskElement, ns, "BuildingId"),
                    LocationId = GetElementValue(taskElement, ns, "LocationId"),
                    CategoryId = GetElementValue(taskElement, ns, "CategoryId"),
                    InstructionId = GetElementValue(taskElement, ns, "InstructionId"),
                    DisciplineId = GetElementValue(taskElement, ns, "DisciplineId"),
                    PriorityId = GetElementValue(taskElement, ns, "PriorityId"),
                    ContractId = GetElementValue(taskElement, ns, "ContractId"),
                    CallerSourceId = GetElementValue(taskElement, ns, "BDET_CALLER_SOURCE_ID"),
                    ReporterName = GetElementValue(taskElement, ns, "ReporterName"),
                    ReporterEmail = GetElementValue(taskElement, ns, "BDET_EMAIL"),
                    Phone = GetElementValue(taskElement, ns, "Phone")
                };

                var raisedDate = GetElementValue(taskElement, ns, "RaisedDateUtc");
                if (DateTime.TryParse(raisedDate, out var parsedRaisedDate))
                    task.RaisedDateUtc = parsedRaisedDate;

                var scheduledDate = GetElementValue(taskElement, ns, "ScheduledDateUtc");
                if (DateTime.TryParse(scheduledDate, out var parsedScheduledDate))
                    task.ScheduledDateUtc = parsedScheduledDate;

                response.Tasks.Add(task);
            }
        }
        catch
        {
            // If parsing fails, return empty response
        }

        return response;
    }

    private static string GetElementValue(XElement parent, XNamespace ns, string elementName)
    {
        var element = parent.Element(ns + elementName) ?? parent.Element(elementName);
        return element?.Value ?? string.Empty;
    }
}

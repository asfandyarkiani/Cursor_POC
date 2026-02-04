using System.Xml.Linq;
using sys_cafm_workorder.DTOs.Api.CAFM;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream response DTO for CAFM GetInstructionSetsByDto SOAP call.
/// </summary>
public class CAFMGetInstructionSetsDownstreamResponseDto
{
    /// <summary>
    /// List of instruction sets parsed from the response.
    /// </summary>
    public List<InstructionSetDto> InstructionSets { get; set; } = new();

    /// <summary>
    /// Raw XML response for debugging.
    /// </summary>
    public string RawResponse { get; set; } = string.Empty;

    /// <summary>
    /// Parses the SOAP XML response to extract instruction sets.
    /// </summary>
    public static CAFMGetInstructionSetsDownstreamResponseDto FromSoapXml(string xml)
    {
        var response = new CAFMGetInstructionSetsDownstreamResponseDto { RawResponse = xml };

        if (string.IsNullOrWhiteSpace(xml))
            return response;

        try
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

            var instructionElements = doc.Descendants(ns + "InstructionSetDto")
                .Concat(doc.Descendants("InstructionSetDto"));

            foreach (var instructionElement in instructionElements)
            {
                var instruction = new InstructionSetDto
                {
                    InstructionId = GetElementValue(instructionElement, ns, "IN_SEQ"),
                    CategoryId = GetElementValue(instructionElement, ns, "IN_FKEY_CAT_SEQ"),
                    DisciplineId = GetElementValue(instructionElement, ns, "IN_FKEY_LAB_SEQ"),
                    PriorityId = GetElementValue(instructionElement, ns, "IN_FKEY_PRI_SEQ"),
                    InstructionName = GetElementValue(instructionElement, ns, "IN_NAME")
                };

                response.InstructionSets.Add(instruction);
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

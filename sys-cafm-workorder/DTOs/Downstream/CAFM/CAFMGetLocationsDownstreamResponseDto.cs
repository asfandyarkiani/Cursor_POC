using System.Xml.Linq;
using sys_cafm_workorder.DTOs.Api.CAFM;

namespace sys_cafm_workorder.DTOs.Downstream.CAFM;

/// <summary>
/// Downstream response DTO for CAFM GetLocationsByDto SOAP call.
/// </summary>
public class CAFMGetLocationsDownstreamResponseDto
{
    /// <summary>
    /// List of locations parsed from the response.
    /// </summary>
    public List<LocationDto> Locations { get; set; } = new();

    /// <summary>
    /// Raw XML response for debugging.
    /// </summary>
    public string RawResponse { get; set; } = string.Empty;

    /// <summary>
    /// Parses the SOAP XML response to extract locations.
    /// </summary>
    public static CAFMGetLocationsDownstreamResponseDto FromSoapXml(string xml)
    {
        var response = new CAFMGetLocationsDownstreamResponseDto { RawResponse = xml };

        if (string.IsNullOrWhiteSpace(xml))
            return response;

        try
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://www.fsi.co.uk/services/evolution/04/09";

            var locationElements = doc.Descendants(ns + "LocationDto")
                .Concat(doc.Descendants("LocationDto"));

            foreach (var locationElement in locationElements)
            {
                var location = new LocationDto
                {
                    LocationId = GetElementValue(locationElement, ns, "LocationId") 
                        ?? GetElementValue(locationElement, ns, "LOC_SEQ"),
                    BuildingId = GetElementValue(locationElement, ns, "BuildingId")
                        ?? GetElementValue(locationElement, ns, "LOC_FKEY_BLD_SEQ"),
                    LocationName = GetElementValue(locationElement, ns, "LOC_NAME"),
                    BuildingName = GetElementValue(locationElement, ns, "LOC_BUILDING_NAME")
                };

                response.Locations.Add(location);
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

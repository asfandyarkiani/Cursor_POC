using Microsoft.Extensions.Logging;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;
using System.Xml;

namespace SystemLayer.Infrastructure.Xml;

public class CafmXmlParser : ICafmXmlParser
{
    private readonly ILogger<CafmXmlParser> _logger;

    public CafmXmlParser(ILogger<CafmXmlParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public LoginResponseDto ParseLoginResponse(string xmlResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            // Check for SOAP fault first
            var faultNode = doc.SelectSingleNode("//soap:Fault", CreateNamespaceManager(doc));
            if (faultNode != null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Errors = new List<string> { ExtractSoapFaultMessage(faultNode) }
                };
            }

            // Parse successful response
            var responseNode = doc.SelectSingleNode("//tem:LoginResponse", CreateNamespaceManager(doc));
            if (responseNode == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid login response format" }
                };
            }

            var sessionToken = GetNodeValue(responseNode, "tem:sessionToken");
            var userId = GetNodeValue(responseNode, "tem:userId");
            var expiresAtStr = GetNodeValue(responseNode, "tem:expiresAt");

            DateTime? expiresAt = null;
            if (DateTime.TryParse(expiresAtStr, out var parsedDate))
            {
                expiresAt = parsedDate;
            }

            return new LoginResponseDto
            {
                Success = !string.IsNullOrEmpty(sessionToken),
                SessionToken = sessionToken,
                UserId = userId,
                ExpiresAt = expiresAt,
                Errors = string.IsNullOrEmpty(sessionToken) ? new List<string> { "No session token received" } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing login response");
            return new LoginResponseDto
            {
                Success = false,
                Errors = new List<string> { $"Parse error: {ex.Message}" }
            };
        }
    }

    public LogoutResponseDto ParseLogoutResponse(string xmlResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            // Check for SOAP fault first
            var faultNode = doc.SelectSingleNode("//soap:Fault", CreateNamespaceManager(doc));
            if (faultNode != null)
            {
                return new LogoutResponseDto
                {
                    Success = false,
                    Errors = new List<string> { ExtractSoapFaultMessage(faultNode) }
                };
            }

            // Parse successful response
            var responseNode = doc.SelectSingleNode("//tem:LogoutResponse", CreateNamespaceManager(doc));
            var message = GetNodeValue(responseNode, "tem:message") ?? "Logout successful";

            return new LogoutResponseDto
            {
                Success = true,
                Message = message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing logout response");
            return new LogoutResponseDto
            {
                Success = false,
                Errors = new List<string> { $"Parse error: {ex.Message}" }
            };
        }
    }

    public CreateWorkOrderResponseDto ParseCreateWorkOrderResponse(string xmlResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            // Check for SOAP fault first
            var faultNode = doc.SelectSingleNode("//soap:Fault", CreateNamespaceManager(doc));
            if (faultNode != null)
            {
                return new CreateWorkOrderResponseDto
                {
                    Success = false,
                    Errors = new List<string> { ExtractSoapFaultMessage(faultNode) }
                };
            }

            // Parse successful response
            var responseNode = doc.SelectSingleNode("//tem:CreateWorkOrderResponse", CreateNamespaceManager(doc));
            if (responseNode == null)
            {
                return new CreateWorkOrderResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid create work order response format" }
                };
            }

            var workOrderId = GetNodeValue(responseNode, "tem:workOrderId");
            var workOrderNumber = GetNodeValue(responseNode, "tem:workOrderNumber");
            var message = GetNodeValue(responseNode, "tem:message");

            return new CreateWorkOrderResponseDto
            {
                Success = !string.IsNullOrEmpty(workOrderId),
                WorkOrderId = workOrderId,
                WorkOrderNumber = workOrderNumber,
                Message = message,
                Errors = string.IsNullOrEmpty(workOrderId) ? new List<string> { "No work order ID received" } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing create work order response");
            return new CreateWorkOrderResponseDto
            {
                Success = false,
                Errors = new List<string> { $"Parse error: {ex.Message}" }
            };
        }
    }

    public GetBreakdownTaskResponseDto ParseGetBreakdownTaskResponse(string xmlResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            // Check for SOAP fault first
            var faultNode = doc.SelectSingleNode("//soap:Fault", CreateNamespaceManager(doc));
            if (faultNode != null)
            {
                return new GetBreakdownTaskResponseDto
                {
                    Success = false,
                    Errors = new List<string> { ExtractSoapFaultMessage(faultNode) }
                };
            }

            // Parse successful response
            var responseNode = doc.SelectSingleNode("//tem:GetBreakdownTaskResponse", CreateNamespaceManager(doc));
            if (responseNode == null)
            {
                return new GetBreakdownTaskResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid breakdown task response format" }
                };
            }

            var taskNode = responseNode.SelectSingleNode("tem:task", CreateNamespaceManager(doc));
            if (taskNode == null)
            {
                return new GetBreakdownTaskResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "No task data found" }
                };
            }

            var requiredSkills = new List<string>();
            var skillsNodes = taskNode.SelectNodes("tem:requiredSkills/tem:skill", CreateNamespaceManager(doc));
            if (skillsNodes != null)
            {
                foreach (XmlNode skillNode in skillsNodes)
                {
                    var skill = skillNode.InnerText;
                    if (!string.IsNullOrEmpty(skill))
                        requiredSkills.Add(skill);
                }
            }

            return new GetBreakdownTaskResponseDto
            {
                Success = true,
                TaskId = GetNodeValue(taskNode, "tem:taskId"),
                TaskName = GetNodeValue(taskNode, "tem:taskName"),
                Description = GetNodeValue(taskNode, "tem:description"),
                Priority = GetNodeValue(taskNode, "tem:priority"),
                EstimatedHours = GetNodeValue(taskNode, "tem:estimatedHours"),
                RequiredSkills = requiredSkills.Any() ? requiredSkills : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing breakdown task response");
            return new GetBreakdownTaskResponseDto
            {
                Success = false,
                Errors = new List<string> { $"Parse error: {ex.Message}" }
            };
        }
    }

    public GetLocationResponseDto ParseGetLocationResponse(string xmlResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            // Check for SOAP fault first
            var faultNode = doc.SelectSingleNode("//soap:Fault", CreateNamespaceManager(doc));
            if (faultNode != null)
            {
                return new GetLocationResponseDto
                {
                    Success = false,
                    Errors = new List<string> { ExtractSoapFaultMessage(faultNode) }
                };
            }

            // Parse successful response
            var responseNode = doc.SelectSingleNode("//tem:GetLocationResponse", CreateNamespaceManager(doc));
            if (responseNode == null)
            {
                return new GetLocationResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid location response format" }
                };
            }

            var locationNode = responseNode.SelectSingleNode("tem:location", CreateNamespaceManager(doc));
            if (locationNode == null)
            {
                return new GetLocationResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "No location data found" }
                };
            }

            return new GetLocationResponseDto
            {
                Success = true,
                LocationId = GetNodeValue(locationNode, "tem:locationId"),
                LocationCode = GetNodeValue(locationNode, "tem:locationCode"),
                LocationName = GetNodeValue(locationNode, "tem:locationName"),
                BuildingId = GetNodeValue(locationNode, "tem:buildingId"),
                BuildingName = GetNodeValue(locationNode, "tem:buildingName"),
                FloorId = GetNodeValue(locationNode, "tem:floorId"),
                FloorName = GetNodeValue(locationNode, "tem:floorName")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing location response");
            return new GetLocationResponseDto
            {
                Success = false,
                Errors = new List<string> { $"Parse error: {ex.Message}" }
            };
        }
    }

    public GetInstructionSetsResponseDto ParseGetInstructionSetsResponse(string xmlResponse)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            // Check for SOAP fault first
            var faultNode = doc.SelectSingleNode("//soap:Fault", CreateNamespaceManager(doc));
            if (faultNode != null)
            {
                return new GetInstructionSetsResponseDto
                {
                    Success = false,
                    Errors = new List<string> { ExtractSoapFaultMessage(faultNode) }
                };
            }

            // Parse successful response
            var responseNode = doc.SelectSingleNode("//tem:GetInstructionSetsResponse", CreateNamespaceManager(doc));
            if (responseNode == null)
            {
                return new GetInstructionSetsResponseDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid instruction sets response format" }
                };
            }

            var instructionSets = new List<InstructionSetDto>();
            var setNodes = responseNode.SelectNodes("tem:instructionSets/tem:instructionSet", CreateNamespaceManager(doc));
            
            if (setNodes != null)
            {
                foreach (XmlNode setNode in setNodes)
                {
                    var steps = new List<InstructionStepDto>();
                    var stepNodes = setNode.SelectNodes("tem:steps/tem:step", CreateNamespaceManager(doc));
                    
                    if (stepNodes != null)
                    {
                        foreach (XmlNode stepNode in stepNodes)
                        {
                            var sequenceStr = GetNodeValue(stepNode, "tem:sequence");
                            int? sequence = null;
                            if (int.TryParse(sequenceStr, out var seq))
                                sequence = seq;

                            steps.Add(new InstructionStepDto
                            {
                                StepId = GetNodeValue(stepNode, "tem:stepId"),
                                Sequence = sequence,
                                Description = GetNodeValue(stepNode, "tem:description"),
                                EstimatedMinutes = GetNodeValue(stepNode, "tem:estimatedMinutes")
                            });
                        }
                    }

                    instructionSets.Add(new InstructionSetDto
                    {
                        InstructionSetId = GetNodeValue(setNode, "tem:instructionSetId"),
                        Name = GetNodeValue(setNode, "tem:name"),
                        Description = GetNodeValue(setNode, "tem:description"),
                        WorkType = GetNodeValue(setNode, "tem:workType"),
                        Steps = steps.Any() ? steps : null
                    });
                }
            }

            return new GetInstructionSetsResponseDto
            {
                Success = true,
                InstructionSets = instructionSets.Any() ? instructionSets : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing instruction sets response");
            return new GetInstructionSetsResponseDto
            {
                Success = false,
                Errors = new List<string> { $"Parse error: {ex.Message}" }
            };
        }
    }

    private static XmlNamespaceManager CreateNamespaceManager(XmlDocument doc)
    {
        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
        nsmgr.AddNamespace("tem", "http://tempuri.org/");
        return nsmgr;
    }

    private static string? GetNodeValue(XmlNode? parentNode, string xpath)
    {
        if (parentNode == null) return null;
        
        var nsmgr = new XmlNamespaceManager(parentNode.OwnerDocument!.NameTable);
        nsmgr.AddNamespace("tem", "http://tempuri.org/");
        
        var node = parentNode.SelectSingleNode(xpath, nsmgr);
        return node?.InnerText;
    }

    private static string ExtractSoapFaultMessage(XmlNode faultNode)
    {
        var faultString = faultNode.SelectSingleNode("faultstring")?.InnerText;
        var faultCode = faultNode.SelectSingleNode("faultcode")?.InnerText;
        
        if (!string.IsNullOrEmpty(faultString))
            return $"SOAP Fault: {faultString}";
        
        if (!string.IsNullOrEmpty(faultCode))
            return $"SOAP Fault Code: {faultCode}";
        
        return "Unknown SOAP fault occurred";
    }
}
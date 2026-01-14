using Microsoft.Extensions.Options;
using SystemLayer.Application.Configuration;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;
using System.Text;
using System.Xml;

namespace SystemLayer.Infrastructure.Xml;

public class CafmXmlBuilder : ICafmXmlBuilder
{
    private readonly CafmConfiguration _config;

    public CafmXmlBuilder(IOptions<CafmConfiguration> config)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
    }

    public string BuildLoginRequest(LoginRequestDto request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">");
        sb.AppendLine("  <soap:Header />");
        sb.AppendLine("  <soap:Body>");
        sb.AppendLine("    <tem:Login>");
        sb.AppendLine($"      <tem:username>{XmlEscape(request.Username)}</tem:username>");
        sb.AppendLine($"      <tem:password>{XmlEscape(request.Password)}</tem:password>");
        sb.AppendLine($"      <tem:database>{XmlEscape(request.Database)}</tem:database>");
        sb.AppendLine("    </tem:Login>");
        sb.AppendLine("  </soap:Body>");
        sb.AppendLine("</soap:Envelope>");
        return sb.ToString();
    }

    public string BuildLogoutRequest(string sessionToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">");
        sb.AppendLine("  <soap:Header />");
        sb.AppendLine("  <soap:Body>");
        sb.AppendLine("    <tem:Logout>");
        sb.AppendLine($"      <tem:sessionToken>{XmlEscape(sessionToken)}</tem:sessionToken>");
        sb.AppendLine("    </tem:Logout>");
        sb.AppendLine("  </soap:Body>");
        sb.AppendLine("</soap:Envelope>");
        return sb.ToString();
    }

    public string BuildCreateWorkOrderRequest(CreateWorkOrderRequestDto request, string sessionToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">");
        sb.AppendLine("  <soap:Header />");
        sb.AppendLine("  <soap:Body>");
        sb.AppendLine("    <tem:CreateWorkOrder>");
        sb.AppendLine($"      <tem:sessionToken>{XmlEscape(sessionToken)}</tem:sessionToken>");
        sb.AppendLine("      <tem:workOrder>");
        sb.AppendLine($"        <tem:workOrderNumber>{XmlEscape(request.WorkOrderNumber)}</tem:workOrderNumber>");
        sb.AppendLine($"        <tem:description>{XmlEscape(request.Description)}</tem:description>");
        sb.AppendLine($"        <tem:priority>{XmlEscape(request.Priority)}</tem:priority>");
        sb.AppendLine($"        <tem:locationId>{XmlEscape(request.LocationId)}</tem:locationId>");
        sb.AppendLine($"        <tem:assetId>{XmlEscape(request.AssetId)}</tem:assetId>");
        sb.AppendLine($"        <tem:requestedBy>{XmlEscape(request.RequestedBy)}</tem:requestedBy>");
        sb.AppendLine($"        <tem:requestedDate>{request.RequestedDate?.ToString("yyyy-MM-ddTHH:mm:ss")}</tem:requestedDate>");
        sb.AppendLine($"        <tem:workType>{XmlEscape(request.WorkType)}</tem:workType>");
        sb.AppendLine($"        <tem:status>{XmlEscape(request.Status)}</tem:status>");
        sb.AppendLine($"        <tem:breakdownTaskId>{XmlEscape(request.BreakdownTaskId)}</tem:breakdownTaskId>");
        
        if (request.InstructionSetIds?.Any() == true)
        {
            sb.AppendLine("        <tem:instructionSetIds>");
            foreach (var id in request.InstructionSetIds)
            {
                sb.AppendLine($"          <tem:id>{XmlEscape(id)}</tem:id>");
            }
            sb.AppendLine("        </tem:instructionSetIds>");
        }
        
        sb.AppendLine("      </tem:workOrder>");
        sb.AppendLine("    </tem:CreateWorkOrder>");
        sb.AppendLine("  </soap:Body>");
        sb.AppendLine("</soap:Envelope>");
        return sb.ToString();
    }

    public string BuildGetBreakdownTaskRequest(GetBreakdownTaskRequestDto request, string sessionToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">");
        sb.AppendLine("  <soap:Header />");
        sb.AppendLine("  <soap:Body>");
        sb.AppendLine("    <tem:GetBreakdownTask>");
        sb.AppendLine($"      <tem:sessionToken>{XmlEscape(sessionToken)}</tem:sessionToken>");
        sb.AppendLine($"      <tem:taskId>{XmlEscape(request.TaskId)}</tem:taskId>");
        sb.AppendLine($"      <tem:assetId>{XmlEscape(request.AssetId)}</tem:assetId>");
        sb.AppendLine($"      <tem:locationId>{XmlEscape(request.LocationId)}</tem:locationId>");
        sb.AppendLine("    </tem:GetBreakdownTask>");
        sb.AppendLine("  </soap:Body>");
        sb.AppendLine("</soap:Envelope>");
        return sb.ToString();
    }

    public string BuildGetLocationRequest(GetLocationRequestDto request, string sessionToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">");
        sb.AppendLine("  <soap:Header />");
        sb.AppendLine("  <soap:Body>");
        sb.AppendLine("    <tem:GetLocation>");
        sb.AppendLine($"      <tem:sessionToken>{XmlEscape(sessionToken)}</tem:sessionToken>");
        sb.AppendLine($"      <tem:locationId>{XmlEscape(request.LocationId)}</tem:locationId>");
        sb.AppendLine($"      <tem:locationCode>{XmlEscape(request.LocationCode)}</tem:locationCode>");
        sb.AppendLine($"      <tem:buildingId>{XmlEscape(request.BuildingId)}</tem:buildingId>");
        sb.AppendLine("    </tem:GetLocation>");
        sb.AppendLine("  </soap:Body>");
        sb.AppendLine("</soap:Envelope>");
        return sb.ToString();
    }

    public string BuildGetInstructionSetsRequest(GetInstructionSetsRequestDto request, string sessionToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sb.AppendLine("<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">");
        sb.AppendLine("  <soap:Header />");
        sb.AppendLine("  <soap:Body>");
        sb.AppendLine("    <tem:GetInstructionSets>");
        sb.AppendLine($"      <tem:sessionToken>{XmlEscape(sessionToken)}</tem:sessionToken>");
        sb.AppendLine($"      <tem:assetId>{XmlEscape(request.AssetId)}</tem:assetId>");
        sb.AppendLine($"      <tem:workType>{XmlEscape(request.WorkType)}</tem:workType>");
        sb.AppendLine($"      <tem:locationId>{XmlEscape(request.LocationId)}</tem:locationId>");
        sb.AppendLine("    </tem:GetInstructionSets>");
        sb.AppendLine("  </soap:Body>");
        sb.AppendLine("</soap:Envelope>");
        return sb.ToString();
    }

    private static string? XmlEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
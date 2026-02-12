using FluentAssertions;
using FacilitiesMgmtSystem.Helper;
using Xunit;

namespace FacilitiesMgmtSystem.Tests.Helper;

/// <summary>
/// Unit tests for SOAPHelper class.
/// </summary>
public class SOAPHelperTests
{
    [Fact]
    public void ContainsSoapFault_WithSoapFault_ShouldReturnTrue()
    {
        // Arrange
        var soapResponse = @"<?xml version=""1.0""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                    <soap:Fault>
                        <faultcode>soap:Server</faultcode>
                        <faultstring>Server error</faultstring>
                    </soap:Fault>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = SOAPHelper.ContainsSoapFault(soapResponse);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsSoapFault_WithoutSoapFault_ShouldReturnFalse()
    {
        // Arrange
        var soapResponse = @"<?xml version=""1.0""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                    <Response>
                        <Success>true</Success>
                    </Response>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = SOAPHelper.ContainsSoapFault(soapResponse);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ExtractSoapFaultMessage_WithFaultString_ShouldReturnMessage()
    {
        // Arrange
        var soapResponse = @"<?xml version=""1.0""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                    <soap:Fault>
                        <faultcode>soap:Server</faultcode>
                        <faultstring>Test error message</faultstring>
                    </soap:Fault>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = SOAPHelper.ExtractSoapFaultMessage(soapResponse);

        // Assert
        result.Should().Be("Test error message");
    }

    [Fact]
    public void ExtractValueFromXml_WithValidElement_ShouldReturnValue()
    {
        // Arrange
        var xml = @"<Response xmlns=""http://tempuri.org/"">
            <SessionId>session-12345</SessionId>
            <Status>Active</Status>
        </Response>";

        // Act
        var sessionId = SOAPHelper.ExtractValueFromXml(xml, "SessionId");
        var status = SOAPHelper.ExtractValueFromXml(xml, "Status");

        // Assert
        sessionId.Should().Be("session-12345");
        status.Should().Be("Active");
    }

    [Fact]
    public void ExtractValueFromXml_WithNonExistentElement_ShouldReturnNull()
    {
        // Arrange
        var xml = @"<Response><Data>value</Data></Response>";

        // Act
        var result = SOAPHelper.ExtractValueFromXml(xml, "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractValueFromXml_WithInvalidXml_ShouldReturnNull()
    {
        // Arrange
        var xml = "not valid xml";

        // Act
        var result = SOAPHelper.ExtractValueFromXml(xml, "Element");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractValueFromXml_WithNestedSoapResponse_ShouldExtractValue()
    {
        // Arrange
        var soapResponse = @"<?xml version=""1.0""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
                <soap:Body>
                    <tem:LoginResponse>
                        <tem:SessionId>abc-123-xyz</tem:SessionId>
                    </tem:LoginResponse>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = SOAPHelper.ExtractValueFromXml(soapResponse, "SessionId");

        // Assert
        result.Should().Be("abc-123-xyz");
    }
}

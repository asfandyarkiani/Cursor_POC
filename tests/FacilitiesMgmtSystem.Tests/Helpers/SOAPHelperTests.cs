using FacilitiesMgmtSystem.Helper;
using FluentAssertions;

namespace FacilitiesMgmtSystem.Tests.Helpers;

public class SOAPHelperTests
{
    [Fact]
    public void ExtractSoapFault_WithSoap11Fault_ReturnsFaultString()
    {
        // Arrange
        var soapFaultResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                    <soap:Fault>
                        <faultcode>soap:Server</faultcode>
                        <faultstring>Test fault message</faultstring>
                    </soap:Fault>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = SOAPHelper.ExtractSoapFault(soapFaultResponse);

        // Assert
        result.Should().Be("Test fault message");
    }

    [Fact]
    public void ExtractSoapFault_WithNoFault_ReturnsNull()
    {
        // Arrange
        var soapResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                    <TestResponse>
                        <Result>Success</Result>
                    </TestResponse>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = SOAPHelper.ExtractSoapFault(soapResponse);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractSoapFault_WithInvalidXml_ReturnsNull()
    {
        // Arrange
        var invalidXml = "not valid xml";

        // Act
        var result = SOAPHelper.ExtractSoapFault(invalidXml);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DeserializeSoapResponse_WithInvalidXml_ReturnsNull()
    {
        // Arrange
        var invalidXml = "not valid xml";

        // Act
        var result = SOAPHelper.DeserializeSoapResponse<TestDto>(invalidXml);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DeserializeSoapResponse_WithEmptyBody_ReturnsNull()
    {
        // Arrange
        var emptyBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = SOAPHelper.DeserializeSoapResponse<TestDto>(emptyBody);

        // Assert
        result.Should().BeNull();
    }

    public class TestDto
    {
        public string? Value { get; set; }
    }
}

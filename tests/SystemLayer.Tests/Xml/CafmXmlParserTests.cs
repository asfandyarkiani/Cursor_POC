using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SystemLayer.Infrastructure.Xml;

namespace SystemLayer.Tests.Xml;

public class CafmXmlParserTests
{
    private readonly CafmXmlParser _xmlParser;
    private readonly Mock<ILogger<CafmXmlParser>> _mockLogger;

    public CafmXmlParserTests()
    {
        _mockLogger = new Mock<ILogger<CafmXmlParser>>();
        _xmlParser = new CafmXmlParser(_mockLogger.Object);
    }

    [Fact]
    public void ParseLoginResponse_WithSuccessfulResponse_ShouldReturnSuccess()
    {
        // Arrange
        var xmlResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
                <soap:Body>
                    <tem:LoginResponse>
                        <tem:sessionToken>abc123def456</tem:sessionToken>
                        <tem:userId>user001</tem:userId>
                        <tem:expiresAt>2024-01-15T15:30:00</tem:expiresAt>
                    </tem:LoginResponse>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = _xmlParser.ParseLoginResponse(xmlResponse);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.SessionToken.Should().Be("abc123def456");
        result.UserId.Should().Be("user001");
        result.ExpiresAt.Should().Be(new DateTime(2024, 1, 15, 15, 30, 0));
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void ParseLoginResponse_WithSoapFault_ShouldReturnFailure()
    {
        // Arrange
        var xmlResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                    <soap:Fault>
                        <faultcode>Client</faultcode>
                        <faultstring>Invalid credentials</faultstring>
                    </soap:Fault>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = _xmlParser.ParseLoginResponse(xmlResponse);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.SessionToken.Should().BeNull();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().Contain("SOAP Fault: Invalid credentials");
    }

    [Fact]
    public void ParseCreateWorkOrderResponse_WithSuccessfulResponse_ShouldReturnSuccess()
    {
        // Arrange
        var xmlResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
                <soap:Body>
                    <tem:CreateWorkOrderResponse>
                        <tem:workOrderId>WO-12345</tem:workOrderId>
                        <tem:workOrderNumber>WO-001</tem:workOrderNumber>
                        <tem:message>Work order created successfully</tem:message>
                    </tem:CreateWorkOrderResponse>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = _xmlParser.ParseCreateWorkOrderResponse(xmlResponse);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WorkOrderId.Should().Be("WO-12345");
        result.WorkOrderNumber.Should().Be("WO-001");
        result.Message.Should().Be("Work order created successfully");
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void ParseGetLocationResponse_WithSuccessfulResponse_ShouldReturnSuccess()
    {
        // Arrange
        var xmlResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
                <soap:Body>
                    <tem:GetLocationResponse>
                        <tem:location>
                            <tem:locationId>LOC-001</tem:locationId>
                            <tem:locationCode>A-101</tem:locationCode>
                            <tem:locationName>Conference Room A</tem:locationName>
                            <tem:buildingId>BLD-001</tem:buildingId>
                            <tem:buildingName>Main Building</tem:buildingName>
                            <tem:floorId>FLR-001</tem:floorId>
                            <tem:floorName>First Floor</tem:floorName>
                        </tem:location>
                    </tem:GetLocationResponse>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = _xmlParser.ParseGetLocationResponse(xmlResponse);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.LocationId.Should().Be("LOC-001");
        result.LocationCode.Should().Be("A-101");
        result.LocationName.Should().Be("Conference Room A");
        result.BuildingId.Should().Be("BLD-001");
        result.BuildingName.Should().Be("Main Building");
        result.FloorId.Should().Be("FLR-001");
        result.FloorName.Should().Be("First Floor");
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void ParseGetInstructionSetsResponse_WithSuccessfulResponse_ShouldReturnSuccess()
    {
        // Arrange
        var xmlResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
                <soap:Body>
                    <tem:GetInstructionSetsResponse>
                        <tem:instructionSets>
                            <tem:instructionSet>
                                <tem:instructionSetId>IS-001</tem:instructionSetId>
                                <tem:name>HVAC Maintenance</tem:name>
                                <tem:description>Standard HVAC maintenance procedures</tem:description>
                                <tem:workType>Preventive</tem:workType>
                                <tem:steps>
                                    <tem:step>
                                        <tem:stepId>STEP-001</tem:stepId>
                                        <tem:sequence>1</tem:sequence>
                                        <tem:description>Check air filters</tem:description>
                                        <tem:estimatedMinutes>15</tem:estimatedMinutes>
                                    </tem:step>
                                    <tem:step>
                                        <tem:stepId>STEP-002</tem:stepId>
                                        <tem:sequence>2</tem:sequence>
                                        <tem:description>Clean coils</tem:description>
                                        <tem:estimatedMinutes>30</tem:estimatedMinutes>
                                    </tem:step>
                                </tem:steps>
                            </tem:instructionSet>
                        </tem:instructionSets>
                    </tem:GetInstructionSetsResponse>
                </soap:Body>
            </soap:Envelope>";

        // Act
        var result = _xmlParser.ParseGetInstructionSetsResponse(xmlResponse);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.InstructionSets.Should().NotBeNull();
        result.InstructionSets.Should().HaveCount(1);
        
        var instructionSet = result.InstructionSets!.First();
        instructionSet.InstructionSetId.Should().Be("IS-001");
        instructionSet.Name.Should().Be("HVAC Maintenance");
        instructionSet.Description.Should().Be("Standard HVAC maintenance procedures");
        instructionSet.WorkType.Should().Be("Preventive");
        instructionSet.Steps.Should().HaveCount(2);
        
        var firstStep = instructionSet.Steps!.First();
        firstStep.StepId.Should().Be("STEP-001");
        firstStep.Sequence.Should().Be(1);
        firstStep.Description.Should().Be("Check air filters");
        firstStep.EstimatedMinutes.Should().Be("15");
    }

    [Fact]
    public void ParseLoginResponse_WithInvalidXml_ShouldReturnFailure()
    {
        // Arrange
        var invalidXml = "This is not valid XML";

        // Act
        var result = _xmlParser.ParseLoginResponse(invalidXml);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().HaveCount(1);
        result.Errors![0].Should().StartWith("Parse error:");
    }
}
using FluentAssertions;
using Microsoft.Extensions.Options;
using SystemLayer.Application.Configuration;
using SystemLayer.Application.DTOs;
using SystemLayer.Infrastructure.Xml;

namespace SystemLayer.Tests.Xml;

public class CafmXmlBuilderTests
{
    private readonly CafmXmlBuilder _xmlBuilder;

    public CafmXmlBuilderTests()
    {
        var config = new CafmConfiguration
        {
            Soap = new SoapConfiguration
            {
                SoapNamespace = "http://tempuri.org/"
            }
        };
        
        _xmlBuilder = new CafmXmlBuilder(Options.Create(config));
    }

    [Fact]
    public void BuildLoginRequest_ShouldGenerateValidXml()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Username = "testuser",
            Password = "testpass",
            Database = "testdb"
        };

        // Act
        var result = _xmlBuilder.BuildLoginRequest(request);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("<tem:username>testuser</tem:username>");
        result.Should().Contain("<tem:password>testpass</tem:password>");
        result.Should().Contain("<tem:database>testdb</tem:database>");
        result.Should().Contain("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        result.Should().Contain("<tem:Login>");
    }

    [Fact]
    public void BuildLoginRequest_ShouldEscapeXmlCharacters()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Username = "test<user>",
            Password = "test&pass",
            Database = "test\"db"
        };

        // Act
        var result = _xmlBuilder.BuildLoginRequest(request);

        // Assert
        result.Should().Contain("<tem:username>test&lt;user&gt;</tem:username>");
        result.Should().Contain("<tem:password>test&amp;pass</tem:password>");
        result.Should().Contain("<tem:database>test&quot;db</tem:database>");
    }

    [Fact]
    public void BuildCreateWorkOrderRequest_ShouldGenerateValidXml()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDto
        {
            WorkOrderNumber = "WO-001",
            Description = "Test work order",
            Priority = "High",
            LocationId = "LOC-001",
            AssetId = "AST-001",
            RequestedBy = "John Doe",
            RequestedDate = new DateTime(2024, 1, 15, 10, 30, 0),
            WorkType = "Maintenance",
            Status = "Open",
            BreakdownTaskId = "BT-001",
            InstructionSetIds = new List<string> { "IS-001", "IS-002" }
        };

        // Act
        var result = _xmlBuilder.BuildCreateWorkOrderRequest(request, "session123");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("<tem:sessionToken>session123</tem:sessionToken>");
        result.Should().Contain("<tem:workOrderNumber>WO-001</tem:workOrderNumber>");
        result.Should().Contain("<tem:description>Test work order</tem:description>");
        result.Should().Contain("<tem:priority>High</tem:priority>");
        result.Should().Contain("<tem:requestedDate>2024-01-15T10:30:00</tem:requestedDate>");
        result.Should().Contain("<tem:id>IS-001</tem:id>");
        result.Should().Contain("<tem:id>IS-002</tem:id>");
        result.Should().Contain("<tem:CreateWorkOrder>");
    }

    [Fact]
    public void BuildLogoutRequest_ShouldGenerateValidXml()
    {
        // Arrange
        var sessionToken = "session123";

        // Act
        var result = _xmlBuilder.BuildLogoutRequest(sessionToken);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("<tem:sessionToken>session123</tem:sessionToken>");
        result.Should().Contain("<tem:Logout>");
        result.Should().Contain("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    }

    [Fact]
    public void BuildGetLocationRequest_ShouldGenerateValidXml()
    {
        // Arrange
        var request = new GetLocationRequestDto
        {
            LocationId = "LOC-001",
            LocationCode = "A-101",
            BuildingId = "BLD-001"
        };

        // Act
        var result = _xmlBuilder.BuildGetLocationRequest(request, "session123");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("<tem:sessionToken>session123</tem:sessionToken>");
        result.Should().Contain("<tem:locationId>LOC-001</tem:locationId>");
        result.Should().Contain("<tem:locationCode>A-101</tem:locationCode>");
        result.Should().Contain("<tem:buildingId>BLD-001</tem:buildingId>");
        result.Should().Contain("<tem:GetLocation>");
    }
}
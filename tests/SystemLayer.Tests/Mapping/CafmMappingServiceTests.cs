using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SystemLayer.Application.DTOs;
using SystemLayer.Infrastructure.Mapping;
using SystemLayer.Infrastructure.Models;

namespace SystemLayer.Tests.Mapping;

public class CafmMappingServiceTests
{
    private readonly CafmMappingService _mappingService;
    private readonly Mock<ILogger<CafmMappingService>> _mockLogger;
    
    public CafmMappingServiceTests()
    {
        _mockLogger = new Mock<ILogger<CafmMappingService>>();
        _mappingService = new CafmMappingService(_mockLogger.Object);
    }
    
    [Fact]
    public void MapToCreateWorkOrderRequest_ShouldMapAllProperties_WhenValidRequest()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            WorkOrderNumber = "WO-12345",
            Description = "Test work order",
            LocationId = "LOC-001",
            Priority = "High",
            AssignedTo = "John Doe",
            ScheduledDate = new DateTime(2024, 1, 15),
            InstructionSetId = "INS-001",
            AdditionalProperties = new Dictionary<string, object>
            {
                { "CustomField1", "Value1" },
                { "CustomField2", 123 }
            }
        };
        var sessionToken = "test-session-token";
        var correlationId = "test-correlation-id";
        
        // Act
        var result = _mappingService.MapToCreateWorkOrderRequest(request, sessionToken, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.SessionToken.Should().Be(sessionToken);
        result.WorkOrderNumber.Should().Be(request.WorkOrderNumber);
        result.Description.Should().Be(request.Description);
        result.LocationId.Should().Be(request.LocationId);
        result.Priority.Should().Be(request.Priority);
        result.AssignedTo.Should().Be(request.AssignedTo);
        result.ScheduledDate.Should().Be(request.ScheduledDate);
        result.InstructionSetId.Should().Be(request.InstructionSetId);
        
        result.CustomFields.Should().HaveCount(2);
        result.CustomFields.Should().Contain(cf => cf.Name == "CustomField1" && cf.Value == "Value1");
        result.CustomFields.Should().Contain(cf => cf.Name == "CustomField2" && cf.Value == "123");
    }
    
    [Fact]
    public void MapFromCreateWorkOrderResponse_ShouldMapSuccessfulResponse_WhenCafmResponseIsSuccessful()
    {
        // Arrange
        var cafmResponse = new CafmCreateWorkOrderResponse
        {
            Success = true,
            WorkOrderId = "12345",
            WorkOrderNumber = "WO-12345",
            Message = "Work order created successfully"
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = _mappingService.MapFromCreateWorkOrderResponse(cafmResponse, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WorkOrderId.Should().Be(cafmResponse.WorkOrderId);
        result.WorkOrderNumber.Should().Be(cafmResponse.WorkOrderNumber);
        result.Message.Should().Be(cafmResponse.Message);
        result.AdditionalData.Should().BeEmpty();
    }
    
    [Fact]
    public void MapFromCreateWorkOrderResponse_ShouldIncludeErrorCode_WhenErrorCodePresent()
    {
        // Arrange
        var cafmResponse = new CafmCreateWorkOrderResponse
        {
            Success = false,
            Message = "Validation failed",
            ErrorCode = "VALIDATION_ERROR"
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = _mappingService.MapFromCreateWorkOrderResponse(cafmResponse, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(cafmResponse.Message);
        result.AdditionalData.Should().ContainKey("ErrorCode");
        result.AdditionalData["ErrorCode"].Should().Be("VALIDATION_ERROR");
    }
    
    [Fact]
    public void MapFromGetLocationResponse_ShouldMapAllProperties_WhenValidResponse()
    {
        // Arrange
        var cafmResponse = new CafmGetLocationResponse
        {
            Success = true,
            Location = new CafmLocation
            {
                LocationId = "LOC-001",
                LocationName = "Building A",
                ParentLocationId = "SITE-001",
                LocationType = "Building",
                Status = "Active",
                Properties = new List<CafmProperty>
                {
                    new() { Name = "Area", Value = "1000" },
                    new() { Name = "Floor", Value = "Ground" }
                }
            }
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = _mappingService.MapFromGetLocationResponse(cafmResponse, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.LocationId.Should().Be("LOC-001");
        result.LocationName.Should().Be("Building A");
        result.ParentLocationId.Should().Be("SITE-001");
        result.LocationType.Should().Be("Building");
        result.Status.Should().Be("Active");
        
        result.Properties.Should().HaveCount(2);
        result.Properties.Should().ContainKey("Area").WhoseValue.Should().Be("1000");
        result.Properties.Should().ContainKey("Floor").WhoseValue.Should().Be("Ground");
    }
    
    [Fact]
    public void MapFromGetLocationResponse_ShouldThrowException_WhenLocationIsNull()
    {
        // Arrange
        var cafmResponse = new CafmGetLocationResponse
        {
            Success = true,
            Location = null
        };
        var correlationId = "test-correlation-id";
        
        // Act & Assert
        var act = () => _mappingService.MapFromGetLocationResponse(cafmResponse, correlationId);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("CAFM location response contains no location data");
    }
    
    [Fact]
    public void MapFromGetBreakdownTaskResponse_ShouldMapWithDuration_WhenDurationProvided()
    {
        // Arrange
        var cafmResponse = new CafmGetBreakdownTaskResponse
        {
            Success = true,
            Task = new CafmBreakdownTask
            {
                TaskId = "TASK-001",
                TaskName = "Pump Maintenance",
                Description = "Replace pump seals",
                Status = "Pending",
                EstimatedDurationMinutes = 120,
                RequiredSkills = new List<string> { "Mechanical", "Hydraulics" },
                TaskDetails = new List<CafmProperty>
                {
                    new() { Name = "Priority", Value = "High" },
                    new() { Name = "Tools", Value = "Wrench Set" }
                }
            }
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = _mappingService.MapFromGetBreakdownTaskResponse(cafmResponse, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.TaskId.Should().Be("TASK-001");
        result.TaskName.Should().Be("Pump Maintenance");
        result.Description.Should().Be("Replace pump seals");
        result.Status.Should().Be("Pending");
        result.EstimatedDuration.Should().Be(TimeSpan.FromMinutes(120));
        
        result.RequiredSkills.Should().HaveCount(2);
        result.RequiredSkills.Should().Contain("Mechanical");
        result.RequiredSkills.Should().Contain("Hydraulics");
        
        result.TaskDetails.Should().HaveCount(2);
        result.TaskDetails.Should().ContainKey("Priority").WhoseValue.Should().Be("High");
        result.TaskDetails.Should().ContainKey("Tools").WhoseValue.Should().Be("Wrench Set");
    }
    
    [Fact]
    public void MapFromGetInstructionSetsResponse_ShouldMapComplexStructure_WhenValidResponse()
    {
        // Arrange
        var cafmResponse = new CafmGetInstructionSetsResponse
        {
            Success = true,
            TotalCount = 2,
            HasMore = false,
            InstructionSets = new List<CafmInstructionSet>
            {
                new()
                {
                    InstructionSetId = "INS-001",
                    Name = "Pump Maintenance",
                    Category = "Mechanical",
                    AssetType = "Pump",
                    Steps = new List<CafmInstructionStep>
                    {
                        new()
                        {
                            StepNumber = 1,
                            Description = "Turn off power",
                            EstimatedDurationMinutes = 5,
                            RequiredTools = new List<string> { "Lockout device" },
                            StepData = new List<CafmProperty>
                            {
                                new() { Name = "Safety", Value = "Critical" }
                            }
                        }
                    },
                    Metadata = new List<CafmProperty>
                    {
                        new() { Name = "Version", Value = "1.2" }
                    }
                }
            }
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = _mappingService.MapFromGetInstructionSetsResponse(cafmResponse, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.HasMore.Should().BeFalse();
        result.InstructionSets.Should().HaveCount(1);
        
        var instructionSet = result.InstructionSets.First();
        instructionSet.InstructionSetId.Should().Be("INS-001");
        instructionSet.Name.Should().Be("Pump Maintenance");
        instructionSet.Category.Should().Be("Mechanical");
        instructionSet.AssetType.Should().Be("Pump");
        
        instructionSet.Steps.Should().HaveCount(1);
        var step = instructionSet.Steps.First();
        step.StepNumber.Should().Be(1);
        step.Description.Should().Be("Turn off power");
        step.EstimatedDuration.Should().Be(TimeSpan.FromMinutes(5));
        step.RequiredTools.Should().Contain("Lockout device");
        step.StepData.Should().ContainKey("Safety").WhoseValue.Should().Be("Critical");
        
        instructionSet.Metadata.Should().ContainKey("Version").WhoseValue.Should().Be("1.2");
    }
}
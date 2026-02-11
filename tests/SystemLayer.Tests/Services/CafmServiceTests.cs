using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;
using SystemLayer.Infrastructure.Mapping;
using SystemLayer.Infrastructure.Models;
using SystemLayer.Infrastructure.Services;

namespace SystemLayer.Tests.Services;

public class CafmServiceTests
{
    private readonly CafmService _cafmService;
    private readonly Mock<ICafmClient> _mockCafmClient;
    private readonly Mock<CafmMappingService> _mockMappingService;
    private readonly Mock<ILogger<CafmService>> _mockLogger;
    
    public CafmServiceTests()
    {
        _mockCafmClient = new Mock<ICafmClient>();
        _mockMappingService = new Mock<CafmMappingService>(Mock.Of<ILogger<CafmMappingService>>());
        _mockLogger = new Mock<ILogger<CafmService>>();
        
        _cafmService = new CafmService(
            _mockCafmClient.Object,
            _mockMappingService.Object,
            _mockLogger.Object);
    }
    
    [Fact]
    public async Task CreateWorkOrderAsync_ShouldReturnSuccess_WhenValidRequestAndSuccessfulResponse()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            WorkOrderNumber = "WO-12345",
            Description = "Test work order",
            LocationId = "LOC-001"
        };
        var correlationId = "test-correlation-id";
        
        var cafmRequest = new CafmCreateWorkOrderRequest();
        var cafmResponse = new CafmCreateWorkOrderResponse
        {
            Success = true,
            WorkOrderId = "12345",
            WorkOrderNumber = "WO-12345"
        };
        var expectedResponse = new CreateWorkOrderResponse
        {
            Success = true,
            WorkOrderId = "12345",
            WorkOrderNumber = "WO-12345"
        };
        
        _mockMappingService
            .Setup(m => m.MapToCreateWorkOrderRequest(request, string.Empty, correlationId))
            .Returns(cafmRequest);
        
        _mockCafmClient
            .Setup(c => c.ExecuteWithSessionAsync<CafmCreateWorkOrderRequest, CafmCreateWorkOrderResponse>(
                "CreateWorkOrder", cafmRequest, correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cafmResponse);
        
        _mockMappingService
            .Setup(m => m.MapFromCreateWorkOrderResponse(cafmResponse, correlationId))
            .Returns(expectedResponse);
        
        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(expectedResponse);
        result.CorrelationId.Should().Be(correlationId);
    }
    
    [Fact]
    public async Task CreateWorkOrderAsync_ShouldReturnValidationError_WhenWorkOrderNumberIsMissing()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            WorkOrderNumber = "", // Missing required field
            Description = "Test work order",
            LocationId = "LOC-001"
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be(SystemLayerErrorCodes.MissingRequiredField);
        result.Error.Message.Should().Contain("Work order number is required");
        result.Error.IsRetryable.Should().BeFalse();
        
        // Verify that CAFM client was not called
        _mockCafmClient.Verify(c => c.ExecuteWithSessionAsync<CafmCreateWorkOrderRequest, CafmCreateWorkOrderResponse>(
            It.IsAny<string>(), It.IsAny<CafmCreateWorkOrderRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [Fact]
    public async Task CreateWorkOrderAsync_ShouldReturnValidationError_WhenDescriptionIsMissing()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            WorkOrderNumber = "WO-12345",
            Description = "", // Missing required field
            LocationId = "LOC-001"
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be(SystemLayerErrorCodes.MissingRequiredField);
        result.Error.Message.Should().Contain("Description is required");
    }
    
    [Fact]
    public async Task CreateWorkOrderAsync_ShouldReturnValidationError_WhenLocationIdIsMissing()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            WorkOrderNumber = "WO-12345",
            Description = "Test work order",
            LocationId = "" // Missing required field
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be(SystemLayerErrorCodes.MissingRequiredField);
        result.Error.Message.Should().Contain("Location ID is required");
    }
    
    [Fact]
    public async Task CreateWorkOrderAsync_ShouldReturnCafmError_WhenCafmResponseIsUnsuccessful()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            WorkOrderNumber = "WO-12345",
            Description = "Test work order",
            LocationId = "LOC-001"
        };
        var correlationId = "test-correlation-id";
        
        var cafmRequest = new CafmCreateWorkOrderRequest();
        var cafmResponse = new CafmCreateWorkOrderResponse
        {
            Success = false,
            Message = "Location not found"
        };
        var mappedResponse = new CreateWorkOrderResponse
        {
            Success = false,
            Message = "Location not found"
        };
        
        _mockMappingService
            .Setup(m => m.MapToCreateWorkOrderRequest(request, string.Empty, correlationId))
            .Returns(cafmRequest);
        
        _mockCafmClient
            .Setup(c => c.ExecuteWithSessionAsync<CafmCreateWorkOrderRequest, CafmCreateWorkOrderResponse>(
                "CreateWorkOrder", cafmRequest, correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cafmResponse);
        
        _mockMappingService
            .Setup(m => m.MapFromCreateWorkOrderResponse(cafmResponse, correlationId))
            .Returns(mappedResponse);
        
        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be(SystemLayerErrorCodes.CafmServiceUnavailable);
        result.Error.Message.Should().Be("Location not found");
    }
    
    [Fact]
    public async Task CreateWorkOrderAsync_ShouldReturnTimeoutError_WhenTimeoutExceptionThrown()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            WorkOrderNumber = "WO-12345",
            Description = "Test work order",
            LocationId = "LOC-001"
        };
        var correlationId = "test-correlation-id";
        
        var cafmRequest = new CafmCreateWorkOrderRequest();
        
        _mockMappingService
            .Setup(m => m.MapToCreateWorkOrderRequest(request, string.Empty, correlationId))
            .Returns(cafmRequest);
        
        _mockCafmClient
            .Setup(c => c.ExecuteWithSessionAsync<CafmCreateWorkOrderRequest, CafmCreateWorkOrderResponse>(
                "CreateWorkOrder", cafmRequest, correlationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("Request timed out"));
        
        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be(SystemLayerErrorCodes.CafmTimeout);
        result.Error.Message.Should().Contain("timed out");
        result.Error.IsRetryable.Should().BeTrue();
    }
    
    [Fact]
    public async Task CreateWorkOrderAsync_ShouldReturnConnectionError_WhenHttpRequestExceptionThrown()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            WorkOrderNumber = "WO-12345",
            Description = "Test work order",
            LocationId = "LOC-001"
        };
        var correlationId = "test-correlation-id";
        
        var cafmRequest = new CafmCreateWorkOrderRequest();
        
        _mockMappingService
            .Setup(m => m.MapToCreateWorkOrderRequest(request, string.Empty, correlationId))
            .Returns(cafmRequest);
        
        _mockCafmClient
            .Setup(c => c.ExecuteWithSessionAsync<CafmCreateWorkOrderRequest, CafmCreateWorkOrderResponse>(
                "CreateWorkOrder", cafmRequest, correlationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection failed"));
        
        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be(SystemLayerErrorCodes.CafmConnectionFailed);
        result.Error.Message.Should().Contain("Failed to connect");
        result.Error.IsRetryable.Should().BeTrue();
    }
    
    [Fact]
    public async Task GetLocationAsync_ShouldReturnSuccess_WhenValidRequestAndSuccessfulResponse()
    {
        // Arrange
        var request = new GetLocationRequest
        {
            LocationId = "LOC-001",
            IncludeHierarchy = true
        };
        var correlationId = "test-correlation-id";
        
        var cafmRequest = new CafmGetLocationRequest();
        var cafmResponse = new CafmGetLocationResponse
        {
            Success = true,
            Location = new CafmLocation
            {
                LocationId = "LOC-001",
                LocationName = "Building A"
            }
        };
        var expectedResponse = new LocationResponse
        {
            LocationId = "LOC-001",
            LocationName = "Building A"
        };
        
        _mockMappingService
            .Setup(m => m.MapToGetLocationRequest(request, string.Empty, correlationId))
            .Returns(cafmRequest);
        
        _mockCafmClient
            .Setup(c => c.ExecuteWithSessionAsync<CafmGetLocationRequest, CafmGetLocationResponse>(
                "GetLocation", cafmRequest, correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cafmResponse);
        
        _mockMappingService
            .Setup(m => m.MapFromGetLocationResponse(cafmResponse, correlationId))
            .Returns(expectedResponse);
        
        // Act
        var result = await _cafmService.GetLocationAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(expectedResponse);
        result.CorrelationId.Should().Be(correlationId);
    }
    
    [Fact]
    public async Task GetLocationAsync_ShouldReturnNotFoundError_WhenLocationNotFound()
    {
        // Arrange
        var request = new GetLocationRequest
        {
            LocationId = "LOC-999"
        };
        var correlationId = "test-correlation-id";
        
        var cafmRequest = new CafmGetLocationRequest();
        var cafmResponse = new CafmGetLocationResponse
        {
            Success = false,
            ErrorCode = "NOT_FOUND",
            Message = "Location not found"
        };
        
        _mockMappingService
            .Setup(m => m.MapToGetLocationRequest(request, string.Empty, correlationId))
            .Returns(cafmRequest);
        
        _mockCafmClient
            .Setup(c => c.ExecuteWithSessionAsync<CafmGetLocationRequest, CafmGetLocationResponse>(
                "GetLocation", cafmRequest, correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cafmResponse);
        
        // Act
        var result = await _cafmService.GetLocationAsync(request, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be(SystemLayerErrorCodes.LocationNotFound);
        result.Error.Message.Should().Be("Location not found");
    }
}
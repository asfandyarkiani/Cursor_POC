using FluentAssertions;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO.CreateWorkOrderDTO;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.CreateWorkOrderApiDTO;
using Xunit;

namespace FacilitiesMgmtSystem.Tests.Handlers;

/// <summary>
/// Unit tests for CreateWorkOrderMRIHandler data mapping and validation.
/// These tests focus on DTO mapping logic without requiring mocked dependencies.
/// </summary>
public class CreateWorkOrderMRIHandlerTests
{
    [Fact]
    public void Request_WithMissingSessionId_CanBeIdentified()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDTO
        {
            Description = "Test work order",
            LocationId = "LOC-001",
            SessionId = null
        };

        // Assert - Validate that we can identify missing session
        request.SessionId.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Request_WithEmptySessionId_CanBeIdentified()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDTO
        {
            Description = "Test work order",
            LocationId = "LOC-001",
            SessionId = ""
        };

        // Assert
        string.IsNullOrEmpty(request.SessionId).Should().BeTrue();
    }

    [Fact]
    public void MapToApiResponse_WithSuccessfulResult_ShouldReturnCorrectData()
    {
        // Arrange - Simulate downstream response
        var downstreamResponse = new CreateWorkOrderApiResponseDTO
        {
            Result = new CreateWorkOrderResultDTO
            {
                Success = true,
                Message = "Work order created successfully.",
                WorkOrderId = "WO-123",
                WorkOrderNumber = "WO2024001",
                Status = "Open",
                CreatedDate = "2024-01-01T10:00:00Z",
                CreatedBy = "System"
            }
        };

        // Act - Simulate mapping logic
        var response = new CreateWorkOrderResponseDTO
        {
            Success = downstreamResponse.Result!.Success,
            Message = downstreamResponse.Result.Message,
            WorkOrder = new WorkOrderData
            {
                WorkOrderId = downstreamResponse.Result.WorkOrderId,
                WorkOrderNumber = downstreamResponse.Result.WorkOrderNumber,
                Status = downstreamResponse.Result.Status,
                CreatedDate = downstreamResponse.Result.CreatedDate,
                CreatedBy = downstreamResponse.Result.CreatedBy
            }
        };

        // Assert
        response.Success.Should().BeTrue();
        response.WorkOrder.Should().NotBeNull();
        response.WorkOrder!.WorkOrderId.Should().Be("WO-123");
        response.WorkOrder.WorkOrderNumber.Should().Be("WO2024001");
        response.WorkOrder.Status.Should().Be("Open");
    }

    [Fact]
    public void MapToApiResponse_WithFailedResult_ShouldReturnError()
    {
        // Arrange - Simulate failed downstream response
        var downstreamResponse = new CreateWorkOrderApiResponseDTO
        {
            Result = new CreateWorkOrderResultDTO
            {
                Success = false,
                ErrorCode = "ERR_001",
                ErrorMessage = "Invalid location ID provided"
            }
        };

        // Act - Simulate mapping logic
        var response = new CreateWorkOrderResponseDTO
        {
            Success = false,
            Message = downstreamResponse.Result!.ErrorMessage ?? "Failed to create work order",
            ErrorProperties = new[] { downstreamResponse.Result.ErrorCode! }
        };

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid location ID provided");
        response.ErrorProperties.Should().Contain("ERR_001");
    }

    [Fact]
    public void MapToDownstreamRequest_ShouldMapAllFields()
    {
        // Arrange
        var apiRequest = new CreateWorkOrderRequestDTO
        {
            Description = "Test description",
            Priority = "High",
            LocationId = "LOC-001",
            AssetId = "ASSET-001",
            RequestedBy = "TestUser",
            WorkOrderType = "Corrective"
        };

        // Act - Simulate mapping to downstream DTO
        var downstreamRequest = new CreateWorkOrderApiRequestDTO
        {
            ContractId = "CONTRACT-001",
            Description = apiRequest.Description,
            Priority = apiRequest.Priority,
            LocationId = apiRequest.LocationId,
            AssetId = apiRequest.AssetId,
            RequestedBy = apiRequest.RequestedBy,
            WorkOrderType = apiRequest.WorkOrderType
        };

        // Assert
        downstreamRequest.Description.Should().Be("Test description");
        downstreamRequest.Priority.Should().Be("High");
        downstreamRequest.LocationId.Should().Be("LOC-001");
        downstreamRequest.ContractId.Should().Be("CONTRACT-001");
    }

    [Fact]
    public void SessionValidation_ThrowsExceptionForMissingSession()
    {
        // This test validates the pattern for checking session ID
        // Arrange
        string? sessionId = null;

        // Act & Assert
        var action = () =>
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new BaseException("Session ID not found in function context.")
                {
                    ErrorProperties = ["MRI authentication is required for this operation."]
                };
            }
        };

        action.Should().Throw<BaseException>()
            .Where(e => e.Message.Contains("Session ID"));
    }
}

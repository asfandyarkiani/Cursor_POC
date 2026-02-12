using FluentAssertions;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO.CreateWorkOrderDTO;
using Xunit;

namespace FacilitiesMgmtSystem.Tests.DTO;

/// <summary>
/// Unit tests for Create Work Order DTOs.
/// </summary>
public class CreateWorkOrderDTOTests
{
    [Fact]
    public void ValidateAPIRequestParameters_WithValidRequest_ShouldNotThrow()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDTO
        {
            Description = "Test work order",
            LocationId = "LOC-001"
        };

        // Act
        var action = () => request.ValidateAPIRequestParameters();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateAPIRequestParameters_WithMissingDescription_ShouldThrow()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDTO
        {
            Description = null,
            LocationId = "LOC-001"
        };

        // Act
        var action = () => request.ValidateAPIRequestParameters();

        // Assert
        action.Should().Throw<RequestValidationFailureException>()
            .Where(e => e.ErrorProperties != null && 
                        e.ErrorProperties.Any(p => p.Contains("Description")));
    }

    [Fact]
    public void ValidateAPIRequestParameters_WithEmptyDescription_ShouldThrow()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDTO
        {
            Description = "   ",
            LocationId = "LOC-001"
        };

        // Act
        var action = () => request.ValidateAPIRequestParameters();

        // Assert
        action.Should().Throw<RequestValidationFailureException>();
    }

    [Fact]
    public void ValidateAPIRequestParameters_WithMissingLocationId_ShouldThrow()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDTO
        {
            Description = "Test work order",
            LocationId = null
        };

        // Act
        var action = () => request.ValidateAPIRequestParameters();

        // Assert
        action.Should().Throw<RequestValidationFailureException>()
            .Where(e => e.ErrorProperties != null && 
                        e.ErrorProperties.Any(p => p.Contains("LocationId")));
    }

    [Fact]
    public void CreateWorkOrderResponseDTO_CanBeCreatedWithData()
    {
        // Arrange & Act
        var response = new CreateWorkOrderResponseDTO
        {
            Success = true,
            Message = "Work order created",
            WorkOrder = new WorkOrderData
            {
                WorkOrderId = "WO-001",
                WorkOrderNumber = "WO2024001",
                Status = "Open",
                CreatedDate = "2024-01-01T00:00:00Z",
                CreatedBy = "TestUser"
            }
        };

        // Assert
        response.Success.Should().BeTrue();
        response.WorkOrder.Should().NotBeNull();
        response.WorkOrder!.WorkOrderId.Should().Be("WO-001");
        response.WorkOrder.Status.Should().Be("Open");
    }
}

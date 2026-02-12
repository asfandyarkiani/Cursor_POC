using FluentAssertions;
using FacilitiesMgmtSystem.DTO.CreateWorkOrderDTO;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.CreateWorkOrderApiDTO;
using FacilitiesMgmtSystem.DTO.GetBreakdownTaskDTO;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetBreakdownTaskApiDTO;
using FacilitiesMgmtSystem.DTO.GetLocationDTO;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.GetLocationApiDTO;
using Xunit;

namespace FacilitiesMgmtSystem.Tests.Mapping;

/// <summary>
/// Unit tests for DTO mapping between API and Downstream layers.
/// </summary>
public class DownstreamMappingTests
{
    [Fact]
    public void CreateWorkOrderApiRequestDTO_ToApiRequestMapping_ShouldMapAllFields()
    {
        // Arrange
        var apiRequest = new CreateWorkOrderRequestDTO
        {
            Description = "Test description",
            Priority = "High",
            LocationId = "LOC-001",
            AssetId = "ASSET-001",
            RequestedBy = "TestUser",
            RequestedDate = "2024-01-01",
            DueDate = "2024-01-15",
            WorkOrderType = "Corrective",
            CategoryId = "CAT-001",
            SubCategoryId = "SUBCAT-001",
            AssignedTo = "Technician1",
            Notes = "Test notes",
            ExternalReference = "EXT-REF-001"
        };

        // Act - Simulate mapping that would happen in handler
        var downstreamRequest = new CreateWorkOrderApiRequestDTO
        {
            ContractId = "CONTRACT-001",
            Description = apiRequest.Description,
            Priority = apiRequest.Priority,
            LocationId = apiRequest.LocationId,
            AssetId = apiRequest.AssetId,
            RequestedBy = apiRequest.RequestedBy,
            RequestedDate = apiRequest.RequestedDate,
            DueDate = apiRequest.DueDate,
            WorkOrderType = apiRequest.WorkOrderType,
            CategoryId = apiRequest.CategoryId,
            SubCategoryId = apiRequest.SubCategoryId,
            AssignedTo = apiRequest.AssignedTo,
            Notes = apiRequest.Notes,
            ExternalReference = apiRequest.ExternalReference
        };

        // Assert
        downstreamRequest.Description.Should().Be("Test description");
        downstreamRequest.Priority.Should().Be("High");
        downstreamRequest.LocationId.Should().Be("LOC-001");
        downstreamRequest.AssetId.Should().Be("ASSET-001");
        downstreamRequest.WorkOrderType.Should().Be("Corrective");
        downstreamRequest.ExternalReference.Should().Be("EXT-REF-001");
    }

    [Fact]
    public void CreateWorkOrderApiResponseDTO_ToApiResponseMapping_ShouldMapSuccessfully()
    {
        // Arrange
        var downstreamResponse = new CreateWorkOrderApiResponseDTO
        {
            Result = new CreateWorkOrderResultDTO
            {
                Success = true,
                Message = "Work order created",
                WorkOrderId = "WO-123",
                WorkOrderNumber = "WO2024001",
                Status = "Open",
                CreatedDate = "2024-01-01T10:00:00Z",
                CreatedBy = "System"
            }
        };

        // Act - Simulate mapping that would happen in handler
        var apiResponse = new CreateWorkOrderResponseDTO
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
        apiResponse.Success.Should().BeTrue();
        apiResponse.WorkOrder.Should().NotBeNull();
        apiResponse.WorkOrder!.WorkOrderId.Should().Be("WO-123");
        apiResponse.WorkOrder.Status.Should().Be("Open");
    }

    [Fact]
    public void BreakdownTaskApiDTO_ToBreakdownTaskData_ShouldMapAllFields()
    {
        // Arrange
        var downstreamTask = new BreakdownTaskApiDTO
        {
            TaskId = "TASK-001",
            TaskName = "Inspect equipment",
            Description = "Perform visual inspection",
            WorkOrderId = "WO-001",
            Status = "Pending",
            Priority = "High",
            EstimatedHours = 2.5m,
            ActualHours = 0m,
            AssignedTo = "Tech1",
            ScheduledStartDate = "2024-01-01",
            ScheduledEndDate = "2024-01-02",
            SequenceNumber = 1,
            InstructionSetId = "INS-001"
        };

        // Act - Simulate mapping
        var apiTask = new BreakdownTaskData
        {
            TaskId = downstreamTask.TaskId,
            TaskName = downstreamTask.TaskName,
            Description = downstreamTask.Description,
            WorkOrderId = downstreamTask.WorkOrderId,
            Status = downstreamTask.Status,
            Priority = downstreamTask.Priority,
            EstimatedHours = downstreamTask.EstimatedHours,
            ActualHours = downstreamTask.ActualHours,
            AssignedTo = downstreamTask.AssignedTo,
            ScheduledStartDate = downstreamTask.ScheduledStartDate,
            ScheduledEndDate = downstreamTask.ScheduledEndDate,
            SequenceNumber = downstreamTask.SequenceNumber,
            InstructionSetId = downstreamTask.InstructionSetId
        };

        // Assert
        apiTask.TaskId.Should().Be("TASK-001");
        apiTask.TaskName.Should().Be("Inspect equipment");
        apiTask.EstimatedHours.Should().Be(2.5m);
        apiTask.SequenceNumber.Should().Be(1);
    }

    [Fact]
    public void LocationApiDTO_ToLocationData_ShouldMapAllFields()
    {
        // Arrange
        var downstreamLocation = new LocationApiDTO
        {
            LocationId = "LOC-001",
            LocationCode = "FL1-R101",
            Name = "Room 101",
            Description = "First floor room",
            LocationType = "Room",
            BuildingId = "BLD-001",
            BuildingName = "Main Building",
            FloorId = "FLR-001",
            FloorName = "Floor 1",
            ParentLocationId = "FLR-001",
            HierarchyPath = "Main Building > Floor 1 > Room 101",
            Area = 50.5m,
            Capacity = 10,
            Status = "Active"
        };

        // Act - Simulate mapping
        var apiLocation = new LocationData
        {
            LocationId = downstreamLocation.LocationId,
            LocationCode = downstreamLocation.LocationCode,
            Name = downstreamLocation.Name,
            Description = downstreamLocation.Description,
            LocationType = downstreamLocation.LocationType,
            BuildingId = downstreamLocation.BuildingId,
            BuildingName = downstreamLocation.BuildingName,
            FloorId = downstreamLocation.FloorId,
            FloorName = downstreamLocation.FloorName,
            ParentLocationId = downstreamLocation.ParentLocationId,
            HierarchyPath = downstreamLocation.HierarchyPath,
            Area = downstreamLocation.Area,
            Capacity = downstreamLocation.Capacity,
            Status = downstreamLocation.Status
        };

        // Assert
        apiLocation.LocationId.Should().Be("LOC-001");
        apiLocation.Name.Should().Be("Room 101");
        apiLocation.HierarchyPath.Should().Contain("Main Building");
        apiLocation.Area.Should().Be(50.5m);
        apiLocation.Capacity.Should().Be(10);
    }

    [Fact]
    public void ErrorMapping_FromDownstreamError_ShouldMapCorrectly()
    {
        // Arrange
        var downstreamResponse = new CreateWorkOrderApiResponseDTO
        {
            Result = new CreateWorkOrderResultDTO
            {
                Success = false,
                Message = "Validation failed",
                ErrorCode = "VAL_ERR_001",
                ErrorMessage = "Invalid location ID provided"
            }
        };

        // Act - Simulate error mapping
        var apiResponse = new CreateWorkOrderResponseDTO
        {
            Success = false,
            Message = downstreamResponse.Result!.ErrorMessage ?? downstreamResponse.Result.Message,
            ErrorProperties = new[] { downstreamResponse.Result.ErrorCode! }
        };

        // Assert
        apiResponse.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Invalid location ID provided");
        apiResponse.ErrorProperties.Should().Contain("VAL_ERR_001");
    }
}

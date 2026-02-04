using FluentAssertions;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO.GetBreakdownTaskDTO;
using Xunit;

namespace FacilitiesMgmtSystem.Tests.DTO;

/// <summary>
/// Unit tests for Get Breakdown Task DTOs.
/// </summary>
public class GetBreakdownTaskDTOTests
{
    [Fact]
    public void ValidateAPIRequestParameters_WithTaskId_ShouldNotThrow()
    {
        // Arrange
        var request = new GetBreakdownTaskRequestDTO
        {
            TaskId = "TASK-001"
        };

        // Act
        var action = () => request.ValidateAPIRequestParameters();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateAPIRequestParameters_WithWorkOrderId_ShouldNotThrow()
    {
        // Arrange
        var request = new GetBreakdownTaskRequestDTO
        {
            WorkOrderId = "WO-001"
        };

        // Act
        var action = () => request.ValidateAPIRequestParameters();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateAPIRequestParameters_WithBothIds_ShouldNotThrow()
    {
        // Arrange
        var request = new GetBreakdownTaskRequestDTO
        {
            TaskId = "TASK-001",
            WorkOrderId = "WO-001"
        };

        // Act
        var action = () => request.ValidateAPIRequestParameters();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void ValidateAPIRequestParameters_WithNoIds_ShouldThrow()
    {
        // Arrange
        var request = new GetBreakdownTaskRequestDTO();

        // Act
        var action = () => request.ValidateAPIRequestParameters();

        // Assert
        action.Should().Throw<RequestValidationFailureException>();
    }

    [Fact]
    public void GetBreakdownTaskResponseDTO_CanBeCreatedWithTasks()
    {
        // Arrange & Act
        var response = new GetBreakdownTaskResponseDTO
        {
            Success = true,
            Message = "Tasks retrieved",
            Tasks = new List<BreakdownTaskData>
            {
                new BreakdownTaskData
                {
                    TaskId = "TASK-001",
                    TaskName = "Inspect equipment",
                    Status = "Pending",
                    Priority = "High",
                    EstimatedHours = 2.5m,
                    SequenceNumber = 1
                },
                new BreakdownTaskData
                {
                    TaskId = "TASK-002",
                    TaskName = "Replace parts",
                    Status = "Pending",
                    Priority = "Medium",
                    EstimatedHours = 4.0m,
                    SequenceNumber = 2
                }
            }
        };

        // Assert
        response.Success.Should().BeTrue();
        response.Tasks.Should().HaveCount(2);
        response.Tasks![0].TaskId.Should().Be("TASK-001");
        response.Tasks[0].EstimatedHours.Should().Be(2.5m);
        response.Tasks[1].SequenceNumber.Should().Be(2);
    }

    [Fact]
    public void BreakdownTaskData_DefaultValues()
    {
        // Arrange & Act
        var task = new BreakdownTaskData();

        // Assert
        task.TaskId.Should().BeNull();
        task.EstimatedHours.Should().BeNull();
        task.SequenceNumber.Should().BeNull();
    }
}

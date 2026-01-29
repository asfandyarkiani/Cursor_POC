using FacilitiesMgmtSystem.DTO;
using FluentAssertions;

namespace FacilitiesMgmtSystem.Tests.DTO;

public class BaseResponseDTOTests
{
    [Fact]
    public void CreateSuccess_WithNoParameters_ReturnsSuccessResponse()
    {
        // Act
        var result = BaseResponseDTO.CreateSuccess();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().BeNull();
        result.Data.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void CreateSuccess_WithMessage_ReturnsSuccessWithMessage()
    {
        // Act
        var result = BaseResponseDTO.CreateSuccess("Operation completed");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Operation completed");
    }

    [Fact]
    public void CreateSuccess_WithData_ReturnsSuccessWithData()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var result = BaseResponseDTO.CreateSuccess(data: data);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void CreateFailure_WithNoParameters_ReturnsFailureResponse()
    {
        // Act
        var result = BaseResponseDTO.CreateFailure();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().BeNull();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void CreateFailure_WithMessage_ReturnsFailureWithMessage()
    {
        // Act
        var result = BaseResponseDTO.CreateFailure("An error occurred");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("An error occurred");
    }

    [Fact]
    public void CreateFailure_WithErrors_ReturnsFailureWithErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = BaseResponseDTO.CreateFailure(errors: errors);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().BeEquivalentTo(errors);
    }
}

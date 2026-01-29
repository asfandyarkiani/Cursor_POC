using FluentAssertions;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using Xunit;

namespace FacilitiesMgmtSystem.Tests.Exceptions;

/// <summary>
/// Unit tests for custom exception classes.
/// </summary>
public class ExceptionTests
{
    [Fact]
    public void RequestValidationFailureException_WithErrorProperties_ShouldStoreProperties()
    {
        // Arrange & Act
        var exception = new RequestValidationFailureException()
        {
            ErrorProperties = new[] { ErrorConstants.INVALID_REQ_PAYLOAD, "Additional error" }
        };

        // Assert
        exception.ErrorProperties.Should().HaveCount(2);
        exception.ErrorProperties.Should().Contain(ErrorConstants.INVALID_REQ_PAYLOAD);
    }

    [Fact]
    public void RequestValidationFailureException_WithMessage_ShouldStoreMessage()
    {
        // Arrange & Act
        var exception = new RequestValidationFailureException("Custom validation message");

        // Assert
        exception.Message.Should().Be("Custom validation message");
    }

    [Fact]
    public void DownStreamApiFailureException_WithStatusCode_ShouldStoreStatusCode()
    {
        // Arrange & Act
        var exception = new DownStreamApiFailureException("API call failed", 502, "Gateway timeout");

        // Assert
        exception.StatusCode.Should().Be(502);
        exception.ResponseBody.Should().Be("Gateway timeout");
        exception.Message.Should().Be("API call failed");
    }

    [Fact]
    public void DownStreamApiFailureException_WithInnerException_ShouldPreserveInnerException()
    {
        // Arrange
        var innerException = new HttpRequestException("Connection refused");

        // Act
        var exception = new DownStreamApiFailureException("Downstream call failed", innerException);

        // Assert
        exception.InnerException.Should().Be(innerException);
        exception.InnerException!.Message.Should().Be("Connection refused");
    }

    [Fact]
    public void ApiException_WithStatusCode_ShouldStoreStatusCode()
    {
        // Arrange & Act
        var exception = new ApiException("Not found", 404);

        // Assert
        exception.StatusCode.Should().Be(404);
        exception.Message.Should().Be("Not found");
    }

    [Fact]
    public void BaseException_DefaultValues_ShouldHaveEmptyErrorProperties()
    {
        // Arrange & Act
        var exception = new BaseException("Test error");

        // Assert
        exception.ErrorCode.Should().BeEmpty();
        exception.ErrorProperties.Should().BeEmpty();
    }

    [Fact]
    public void BaseException_WithErrorCode_ShouldStoreErrorCode()
    {
        // Arrange & Act
        var exception = new BaseException("Test error")
        {
            ErrorCode = "ERR_001"
        };

        // Assert
        exception.ErrorCode.Should().Be("ERR_001");
    }
}

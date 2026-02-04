using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.DTO.NetworkTestDTO;
using FacilitiesMgmtSystem.Implementations.MRI.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FacilitiesMgmtSystem.Tests.Handlers;

public class NetworkTestMRIHandlerTests
{
    private readonly Mock<ILogger<NetworkTestMRIHandler>> _loggerMock;
    private readonly NetworkTestMRIHandler _handler;

    public NetworkTestMRIHandlerTests()
    {
        _loggerMock = new Mock<ILogger<NetworkTestMRIHandler>>();
        _handler = new NetworkTestMRIHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task ProcessRequest_WithNullRequest_ReturnsSuccessWithMessage()
    {
        // Act
        var result = await _handler.ProcessRequest(null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(InfoConstants.NETWORK_TEST_SUCCESS);
        result.Data.Should().BeOfType<NetworkTestResponseDTO>();
    }

    [Fact]
    public async Task ProcessRequest_WithRequest_ReturnsSuccessWithMessage()
    {
        // Arrange
        var request = new NetworkTestRequestDTO
        {
            CorrelationId = "test-correlation-123"
        };

        // Act
        var result = await _handler.ProcessRequest(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(InfoConstants.NETWORK_TEST_SUCCESS);
        
        var responseData = result.Data as NetworkTestResponseDTO;
        responseData.Should().NotBeNull();
        responseData!.CorrelationId.Should().Be("test-correlation-123");
    }

    [Fact]
    public async Task ProcessRequest_ResponseContainsTimestamp()
    {
        // Arrange
        var beforeTest = DateTime.UtcNow;

        // Act
        var result = await _handler.ProcessRequest(null);

        // Assert
        var responseData = result.Data as NetworkTestResponseDTO;
        responseData.Should().NotBeNull();
        responseData!.Timestamp.Should().BeOnOrAfter(beforeTest);
        responseData.Timestamp.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task ProcessRequest_ResponseMessageMatchesConstant()
    {
        // Act
        var result = await _handler.ProcessRequest(null);

        // Assert
        var responseData = result.Data as NetworkTestResponseDTO;
        responseData.Should().NotBeNull();
        responseData!.Message.Should().Be("Test is successful!!!");
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SystemLayer.Application.Configuration;
using SystemLayer.Application.DTOs;
using SystemLayer.Application.Interfaces;
using SystemLayer.Infrastructure.Services;

namespace SystemLayer.Tests.Services;

public class CafmServiceTests
{
    private readonly Mock<ICafmClient> _mockCafmClient;
    private readonly Mock<ICafmAuthenticationService> _mockAuthService;
    private readonly Mock<ICafmXmlBuilder> _mockXmlBuilder;
    private readonly Mock<ICafmXmlParser> _mockXmlParser;
    private readonly Mock<ILogger<CafmService>> _mockLogger;
    private readonly CafmService _cafmService;

    public CafmServiceTests()
    {
        _mockCafmClient = new Mock<ICafmClient>();
        _mockAuthService = new Mock<ICafmAuthenticationService>();
        _mockXmlBuilder = new Mock<ICafmXmlBuilder>();
        _mockXmlParser = new Mock<ICafmXmlParser>();
        _mockLogger = new Mock<ILogger<CafmService>>();

        var config = new CafmConfiguration
        {
            Username = "testuser",
            Password = "testpass",
            Database = "testdb",
            Soap = new SoapConfiguration
            {
                CreateWorkOrderAction = "CreateWorkOrder"
            }
        };

        _cafmService = new CafmService(
            _mockCafmClient.Object,
            _mockAuthService.Object,
            _mockXmlBuilder.Object,
            _mockXmlParser.Object,
            _mockLogger.Object,
            Options.Create(config));
    }

    [Fact]
    public async Task CreateWorkOrderAsync_WithSuccessfulLogin_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDto
        {
            WorkOrderNumber = "WO-001",
            Description = "Test work order"
        };

        var loginResponse = new LoginResponseDto
        {
            Success = true,
            SessionToken = "session123"
        };

        var workOrderResponse = new CreateWorkOrderResponseDto
        {
            Success = true,
            WorkOrderId = "WO-12345"
        };

        var logoutResponse = new LogoutResponseDto { Success = true };

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        _mockXmlBuilder
            .Setup(x => x.BuildCreateWorkOrderRequest(request, "session123"))
            .Returns("<xml>test</xml>");

        _mockCafmClient
            .Setup(x => x.SendSoapRequestAsync("CreateWorkOrder", "<xml>test</xml>", "session123", It.IsAny<CancellationToken>()))
            .ReturnsAsync("<response>success</response>");

        _mockXmlParser
            .Setup(x => x.ParseCreateWorkOrderResponse("<response>success</response>"))
            .Returns(workOrderResponse);

        _mockAuthService
            .Setup(x => x.LogoutAsync("session123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(logoutResponse);

        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WorkOrderId.Should().Be("WO-12345");

        // Verify all services were called
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<LoginRequestDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAuthService.Verify(x => x.LogoutAsync("session123", It.IsAny<CancellationToken>()), Times.Once);
        _mockCafmClient.Verify(x => x.SendSoapRequestAsync("CreateWorkOrder", "<xml>test</xml>", "session123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_WithFailedLogin_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDto
        {
            WorkOrderNumber = "WO-001",
            Description = "Test work order"
        };

        var loginResponse = new LoginResponseDto
        {
            Success = false,
            Errors = new List<string> { "Invalid credentials" }
        };

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid credentials");

        // Verify login was called but not the work order creation
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<LoginRequestDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCafmClient.Verify(x => x.SendSoapRequestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockAuthService.Verify(x => x.LogoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateWorkOrderAsync_WithExceptionDuringOperation_ShouldLogoutAndReturnFailure()
    {
        // Arrange
        var request = new CreateWorkOrderRequestDto
        {
            WorkOrderNumber = "WO-001",
            Description = "Test work order"
        };

        var loginResponse = new LoginResponseDto
        {
            Success = true,
            SessionToken = "session123"
        };

        var logoutResponse = new LogoutResponseDto { Success = true };

        _mockAuthService
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        _mockXmlBuilder
            .Setup(x => x.BuildCreateWorkOrderRequest(request, "session123"))
            .Throws(new Exception("XML build failed"));

        _mockAuthService
            .Setup(x => x.LogoutAsync("session123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(logoutResponse);

        // Act
        var result = await _cafmService.CreateWorkOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Operation failed: XML build failed");

        // Verify logout was still called despite the exception
        _mockAuthService.Verify(x => x.LogoutAsync("session123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldDelegateToAuthenticationService()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Username = "testuser",
            Password = "testpass",
            Database = "testdb"
        };

        var expectedResponse = new LoginResponseDto
        {
            Success = true,
            SessionToken = "session123"
        };

        _mockAuthService
            .Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _cafmService.LoginAsync(request);

        // Assert
        result.Should().Be(expectedResponse);
        _mockAuthService.Verify(x => x.LoginAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldDelegateToAuthenticationService()
    {
        // Arrange
        var request = new LogoutRequestDto
        {
            SessionToken = "session123"
        };

        var expectedResponse = new LogoutResponseDto
        {
            Success = true,
            Message = "Logout successful"
        };

        _mockAuthService
            .Setup(x => x.LogoutAsync("session123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _cafmService.LogoutAsync(request);

        // Assert
        result.Should().Be(expectedResponse);
        _mockAuthService.Verify(x => x.LogoutAsync("session123", It.IsAny<CancellationToken>()), Times.Once);
    }
}
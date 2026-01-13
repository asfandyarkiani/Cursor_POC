using FluentAssertions;
using Microsoft.Extensions.Options;
using SystemLayer.Infrastructure.Configuration;
using SystemLayer.Infrastructure.DependencyInjection;

namespace SystemLayer.Tests.Configuration;

public class ConfigurationValidationTests
{
    [Fact]
    public void CafmOptions_ShouldBeValid_WhenAllRequiredPropertiesSet()
    {
        // Arrange
        var options = new CafmOptions
        {
            BaseUrl = "https://cafm.example.com",
            ServicePath = "/services/CafmService.asmx",
            Username = "testuser",
            Password = "testpass",
            Database = "testdb"
        };
        
        var validator = new ValidateOptionsService<CafmOptions>();
        
        // Act
        var result = validator.Validate(null, options);
        
        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        if (result.Failures != null)
        {
            result.Failures.Should().BeEmpty();
        }
    }
    
    [Fact]
    public void CafmOptions_ShouldBeInvalid_WhenBaseUrlIsMissing()
    {
        // Arrange
        var options = new CafmOptions
        {
            BaseUrl = "", // Missing required field
            ServicePath = "/services/CafmService.asmx",
            Username = "testuser",
            Password = "testpass",
            Database = "testdb"
        };
        
        var validator = new ValidateOptionsService<CafmOptions>();
        
        // Act
        var result = validator.Validate(null, options);
        
        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().NotBeEmpty();
        result.Failures.Should().Contain(f => f.Contains("BaseUrl"));
    }
    
    [Fact]
    public void CafmOptions_ShouldBeInvalid_WhenBaseUrlIsInvalidUrl()
    {
        // Arrange
        var options = new CafmOptions
        {
            BaseUrl = "not-a-valid-url", // Invalid URL format
            ServicePath = "/services/CafmService.asmx",
            Username = "testuser",
            Password = "testpass",
            Database = "testdb"
        };
        
        var validator = new ValidateOptionsService<CafmOptions>();
        
        // Act
        var result = validator.Validate(null, options);
        
        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().NotBeEmpty();
    }
    
    [Fact]
    public void CafmOptions_ShouldBeInvalid_WhenTimeoutSecondsOutOfRange()
    {
        // Arrange
        var options = new CafmOptions
        {
            BaseUrl = "https://cafm.example.com",
            ServicePath = "/services/CafmService.asmx",
            Username = "testuser",
            Password = "testpass",
            Database = "testdb",
            TimeoutSeconds = 0 // Out of range (should be 1-300)
        };
        
        var validator = new ValidateOptionsService<CafmOptions>();
        
        // Act
        var result = validator.Validate(null, options);
        
        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().NotBeEmpty();
        result.Failures.Should().Contain(f => f.Contains("TimeoutSeconds"));
    }
    
    [Fact]
    public void CafmOptions_ShouldBeInvalid_WhenMaxRetryAttemptsOutOfRange()
    {
        // Arrange
        var options = new CafmOptions
        {
            BaseUrl = "https://cafm.example.com",
            ServicePath = "/services/CafmService.asmx",
            Username = "testuser",
            Password = "testpass",
            Database = "testdb",
            MaxRetryAttempts = 15 // Out of range (should be 0-10)
        };
        
        var validator = new ValidateOptionsService<CafmOptions>();
        
        // Act
        var result = validator.Validate(null, options);
        
        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Failures.Should().NotBeEmpty();
        result.Failures.Should().Contain(f => f.Contains("MaxRetryAttempts"));
    }
    
    [Fact]
    public void ResilienceOptions_ShouldBeValid_WhenAllPropertiesInRange()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            Retry = new RetryPolicyOptions
            {
                MaxAttempts = 3,
                BaseDelayMs = 1000,
                MaxDelayMs = 10000
            },
            CircuitBreaker = new CircuitBreakerPolicyOptions
            {
                FailureThreshold = 5,
                DurationSeconds = 30,
                MinimumThroughput = 10
            },
            Timeout = new TimeoutPolicyOptions
            {
                TimeoutSeconds = 30
            }
        };
        
        var validator = new ValidateOptionsService<ResilienceOptions>();
        
        // Act
        var result = validator.Validate(null, options);
        
        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        if (result.Failures != null)
        {
            result.Failures.Should().BeEmpty();
        }
    }
    
    [Fact]
    public void KeyVaultOptions_ShouldBeValid_WhenDisabled()
    {
        // Arrange
        var options = new KeyVaultOptions
        {
            Enabled = false,
            VaultUrl = null,
            ClientId = null
        };
        
        var validator = new ValidateOptionsService<KeyVaultOptions>();
        
        // Act
        var result = validator.Validate(null, options);
        
        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        if (result.Failures != null)
        {
            result.Failures.Should().BeEmpty();
        }
    }
    
    [Fact]
    public void KeyVaultOptions_ShouldBeValid_WhenEnabledWithValidUrl()
    {
        // Arrange
        var options = new KeyVaultOptions
        {
            Enabled = true,
            VaultUrl = "https://test-keyvault.vault.azure.net/",
            ClientId = "12345678-1234-1234-1234-123456789012"
        };
        
        var validator = new ValidateOptionsService<KeyVaultOptions>();
        
        // Act
        var result = validator.Validate(null, options);
        
        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        if (result.Failures != null)
        {
            result.Failures.Should().BeEmpty();
        }
    }
}
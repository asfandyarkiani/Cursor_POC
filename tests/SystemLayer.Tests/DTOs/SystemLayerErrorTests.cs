using FluentAssertions;
using SystemLayer.Application.DTOs;

namespace SystemLayer.Tests.DTOs;

public class SystemLayerErrorTests
{
    [Fact]
    public void SystemLayerResult_Ok_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var data = new CreateWorkOrderResponse { Success = true, WorkOrderId = "12345" };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = SystemLayerResult<CreateWorkOrderResponse>.Ok(data, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(data);
        result.Error.Should().BeNull();
        result.CorrelationId.Should().Be(correlationId);
    }
    
    [Fact]
    public void SystemLayerResult_Fail_WithError_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new SystemLayerError
        {
            ErrorCode = SystemLayerErrorCodes.CafmConnectionFailed,
            Message = "Connection failed",
            IsRetryable = true
        };
        var correlationId = "test-correlation-id";
        
        // Act
        var result = SystemLayerResult<CreateWorkOrderResponse>.Fail(error, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().Be(error);
        result.CorrelationId.Should().Be(correlationId);
    }
    
    [Fact]
    public void SystemLayerResult_Fail_WithErrorCodeAndMessage_ShouldCreateFailedResult()
    {
        // Arrange
        var errorCode = SystemLayerErrorCodes.InvalidRequest;
        var message = "Invalid request data";
        var isRetryable = false;
        var correlationId = "test-correlation-id";
        
        // Act
        var result = SystemLayerResult<CreateWorkOrderResponse>.Fail(errorCode, message, isRetryable, correlationId);
        
        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.ErrorCode.Should().Be(errorCode);
        result.Error.Message.Should().Be(message);
        result.Error.IsRetryable.Should().Be(isRetryable);
        result.Error.CorrelationId.Should().Be(correlationId);
        result.CorrelationId.Should().Be(correlationId);
    }
    
    [Fact]
    public void SystemLayerError_ShouldSetTimestamp_WhenCreated()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        
        // Act
        var error = new SystemLayerError
        {
            ErrorCode = SystemLayerErrorCodes.InternalError,
            Message = "Test error"
        };
        
        var afterCreation = DateTime.UtcNow;
        
        // Assert
        error.Timestamp.Should().BeAfter(beforeCreation.AddSeconds(-1));
        error.Timestamp.Should().BeBefore(afterCreation.AddSeconds(1));
    }
    
    [Theory]
    [InlineData(SystemLayerErrorCodes.AuthenticationFailed)]
    [InlineData(SystemLayerErrorCodes.CafmConnectionFailed)]
    [InlineData(SystemLayerErrorCodes.InvalidRequest)]
    [InlineData(SystemLayerErrorCodes.WorkOrderNotFound)]
    [InlineData(SystemLayerErrorCodes.InternalError)]
    public void SystemLayerErrorCodes_ShouldHaveValidConstantValues(string errorCode)
    {
        // Assert
        errorCode.Should().NotBeNullOrEmpty();
        errorCode.Should().StartWith("SL_");
        errorCode.Should().Contain("_");
    }
    
    [Fact]
    public void SystemLayerErrorCodes_ShouldHaveUniqueValues()
    {
        // Arrange
        var errorCodeFields = typeof(SystemLayerErrorCodes)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string))
            .Select(f => f.GetValue(null) as string)
            .Where(v => v != null)
            .ToList();
        
        // Assert
        errorCodeFields.Should().NotBeEmpty();
        errorCodeFields.Should().OnlyHaveUniqueItems();
    }
    
    [Fact]
    public void SystemLayerErrorCodes_AuthenticationErrors_ShouldHaveCorrectPrefix()
    {
        // Assert
        SystemLayerErrorCodes.AuthenticationFailed.Should().StartWith("SL_AUTH_");
        SystemLayerErrorCodes.AuthorizationFailed.Should().StartWith("SL_AUTH_");
        SystemLayerErrorCodes.TokenExpired.Should().StartWith("SL_AUTH_");
    }
    
    [Fact]
    public void SystemLayerErrorCodes_CafmErrors_ShouldHaveCorrectPrefix()
    {
        // Assert
        SystemLayerErrorCodes.CafmConnectionFailed.Should().StartWith("SL_CAFM_");
        SystemLayerErrorCodes.CafmTimeout.Should().StartWith("SL_CAFM_");
        SystemLayerErrorCodes.CafmSoapFault.Should().StartWith("SL_CAFM_");
        SystemLayerErrorCodes.CafmInvalidResponse.Should().StartWith("SL_CAFM_");
        SystemLayerErrorCodes.CafmServiceUnavailable.Should().StartWith("SL_CAFM_");
    }
    
    [Fact]
    public void SystemLayerErrorCodes_ValidationErrors_ShouldHaveCorrectPrefix()
    {
        // Assert
        SystemLayerErrorCodes.InvalidRequest.Should().StartWith("SL_VAL_");
        SystemLayerErrorCodes.MissingRequiredField.Should().StartWith("SL_VAL_");
        SystemLayerErrorCodes.InvalidFieldValue.Should().StartWith("SL_VAL_");
    }
    
    [Fact]
    public void SystemLayerErrorCodes_BusinessErrors_ShouldHaveCorrectPrefix()
    {
        // Assert
        SystemLayerErrorCodes.WorkOrderNotFound.Should().StartWith("SL_BIZ_");
        SystemLayerErrorCodes.LocationNotFound.Should().StartWith("SL_BIZ_");
        SystemLayerErrorCodes.TaskNotFound.Should().StartWith("SL_BIZ_");
        SystemLayerErrorCodes.InstructionSetNotFound.Should().StartWith("SL_BIZ_");
        SystemLayerErrorCodes.DuplicateWorkOrder.Should().StartWith("SL_BIZ_");
    }
    
    [Fact]
    public void SystemLayerErrorCodes_SystemErrors_ShouldHaveCorrectPrefix()
    {
        // Assert
        SystemLayerErrorCodes.InternalError.Should().StartWith("SL_SYS_");
        SystemLayerErrorCodes.ConfigurationError.Should().StartWith("SL_SYS_");
        SystemLayerErrorCodes.MappingError.Should().StartWith("SL_SYS_");
        SystemLayerErrorCodes.CircuitBreakerOpen.Should().StartWith("SL_SYS_");
    }
}
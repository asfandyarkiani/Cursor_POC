using SystemLayer.Application.DTOs;

namespace SystemLayer.Application.Interfaces;

public interface ICafmClient
{
    Task<string> SendSoapRequestAsync(string soapAction, string xmlRequest, string? sessionToken = null, CancellationToken cancellationToken = default);
}

public interface ICafmAuthenticationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<LogoutResponseDto> LogoutAsync(string sessionToken, CancellationToken cancellationToken = default);
}

public interface ICafmXmlBuilder
{
    string BuildLoginRequest(LoginRequestDto request);
    string BuildLogoutRequest(string sessionToken);
    string BuildCreateWorkOrderRequest(CreateWorkOrderRequestDto request, string sessionToken);
    string BuildGetBreakdownTaskRequest(GetBreakdownTaskRequestDto request, string sessionToken);
    string BuildGetLocationRequest(GetLocationRequestDto request, string sessionToken);
    string BuildGetInstructionSetsRequest(GetInstructionSetsRequestDto request, string sessionToken);
}

public interface ICafmXmlParser
{
    LoginResponseDto ParseLoginResponse(string xmlResponse);
    LogoutResponseDto ParseLogoutResponse(string xmlResponse);
    CreateWorkOrderResponseDto ParseCreateWorkOrderResponse(string xmlResponse);
    GetBreakdownTaskResponseDto ParseGetBreakdownTaskResponse(string xmlResponse);
    GetLocationResponseDto ParseGetLocationResponse(string xmlResponse);
    GetInstructionSetsResponseDto ParseGetInstructionSetsResponse(string xmlResponse);
}
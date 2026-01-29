using SystemLayer.Application.DTOs;

namespace SystemLayer.Application.Interfaces;

public interface ICafmService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<LogoutResponseDto> LogoutAsync(LogoutRequestDto request, CancellationToken cancellationToken = default);
    Task<CreateWorkOrderResponseDto> CreateWorkOrderAsync(CreateWorkOrderRequestDto request, CancellationToken cancellationToken = default);
    Task<GetBreakdownTaskResponseDto> GetBreakdownTaskAsync(GetBreakdownTaskRequestDto request, CancellationToken cancellationToken = default);
    Task<GetLocationResponseDto> GetLocationAsync(GetLocationRequestDto request, CancellationToken cancellationToken = default);
    Task<GetInstructionSetsResponseDto> GetInstructionSetsAsync(GetInstructionSetsRequestDto request, CancellationToken cancellationToken = default);
}
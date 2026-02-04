using AlGhurair.Core.SystemLayer.Handlers;
using AlGhurair.SystemLayer.OracleFusionHCM.Constants;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.AtomicHandlerDTOs;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.CreateLeaveDTO;
using AlGhurair.SystemLayer.OracleFusionHCM.DTO.DownstreamDTOs;
using Microsoft.Extensions.Logging;

namespace AlGhurair.SystemLayer.OracleFusionHCM.Implementations.OracleFusionHCM.Handlers;

/// <summary>
/// Handler for creating leave in Oracle Fusion HCM
/// Orchestrates transformation and atomic handler execution
/// </summary>
public class CreateLeaveHandler : IBaseHandler<CreateLeaveReqDTO, CreateLeaveResDTO>
{
    private readonly IAtomicHandler<CreateLeaveHandlerReqDTO, CreateLeaveApiResDTO> _createLeaveAtomicHandler;
    private readonly ILogger<CreateLeaveHandler> _logger;

    public CreateLeaveHandler(
        IAtomicHandler<CreateLeaveHandlerReqDTO, CreateLeaveApiResDTO> createLeaveAtomicHandler,
        ILogger<CreateLeaveHandler> logger)
    {
        _createLeaveAtomicHandler = createLeaveAtomicHandler ?? throw new ArgumentNullException(nameof(createLeaveAtomicHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles leave creation request
    /// Transforms D365 format to Oracle HCM format and calls atomic handler
    /// </summary>
    public async Task<CreateLeaveResDTO> HandleAsync(CreateLeaveReqDTO request)
    {
        _logger.LogInformation(InfoConstants.LEAVE_REQUEST_RECEIVED);

        // Validate request
        (bool isValid, string errorMessage) = request.Validate();
        if (!isValid)
        {
            _logger.LogError($"{ErrorConstants.OFH_LVEMGT_0002}: {errorMessage}");
            return new CreateLeaveResDTO
            {
                Status = "failure",
                Message = $"{ErrorConstants.OFH_LVEMGT_0002}: {errorMessage}",
                PersonAbsenceEntryId = null,
                Success = "false"
            };
        }

        try
        {
            // Transform D365 request to Oracle HCM format
            CreateLeaveHandlerReqDTO handlerRequest = TransformToHandlerRequest(request);

            // Call atomic handler to create leave in Oracle Fusion HCM
            CreateLeaveApiResDTO apiResponse = await _createLeaveAtomicHandler.ExecuteAsync(handlerRequest);

            // Transform Oracle HCM response to D365 format
            CreateLeaveResDTO response = TransformToResponse(apiResponse);

            _logger.LogInformation(InfoConstants.LEAVE_CREATED_SUCCESSFULLY);

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"{ErrorConstants.OFH_LVEMGT_0003}: Oracle Fusion HCM API call failed");
            return new CreateLeaveResDTO
            {
                Status = "failure",
                Message = $"{ErrorConstants.OFH_LVEMGT_0003}: {ex.Message}",
                PersonAbsenceEntryId = null,
                Success = "false"
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{ErrorConstants.OFH_LVEMGT_0001}: Failed to create leave");
            return new CreateLeaveResDTO
            {
                Status = "failure",
                Message = $"{ErrorConstants.OFH_LVEMGT_0001}: {ex.Message}",
                PersonAbsenceEntryId = null,
                Success = "false"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{ErrorConstants.OFH_LVEMGT_0001}: Unexpected error creating leave");
            return new CreateLeaveResDTO
            {
                Status = "failure",
                Message = $"{ErrorConstants.OFH_LVEMGT_0001}: {ex.Message}",
                PersonAbsenceEntryId = null,
                Success = "false"
            };
        }
    }

    /// <summary>
    /// Transforms D365 request to Oracle HCM format
    /// Maps field names: employeeNumber → personNumber, absenceStatusCode → absenceStatusCd, etc.
    /// </summary>
    private CreateLeaveHandlerReqDTO TransformToHandlerRequest(CreateLeaveReqDTO request)
    {
        return new CreateLeaveHandlerReqDTO
        {
            PersonNumber = request.EmployeeNumber.ToString(),
            AbsenceType = request.AbsenceType,
            Employer = request.Employer,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            AbsenceStatusCd = request.AbsenceStatusCode,
            ApprovalStatusCd = request.ApprovalStatusCode,
            StartDateDuration = request.StartDateDuration,
            EndDateDuration = request.EndDateDuration
        };
    }

    /// <summary>
    /// Transforms Oracle HCM response to D365 format
    /// Extracts PersonAbsenceEntryId and creates success response
    /// </summary>
    private CreateLeaveResDTO TransformToResponse(CreateLeaveApiResDTO apiResponse)
    {
        return new CreateLeaveResDTO
        {
            Status = "success",
            Message = "Data successfully sent to Oracle Fusion",
            PersonAbsenceEntryId = apiResponse.PersonAbsenceEntryId,
            Success = "true"
        };
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FacilitiesMgmtSystem.ConfigModels;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.DTO.CreateWorkOrderDTO;
using FacilitiesMgmtSystem.DTO.DownsteamDTOs.CreateWorkOrderApiDTO;
using FacilitiesMgmtSystem.Helper;
using FacilitiesMgmtSystem.Implementations.MRI.AtomicHandlers.CreateWorkOrderAtomicHandler;

namespace FacilitiesMgmtSystem.Implementations.MRI.Handlers;

/// <summary>
/// MRI Handler for Create Work Order operation.
/// Orchestrates the request processing and data transformation.
/// </summary>
public class CreateWorkOrderMRIHandler
{
    private readonly ILogger<CreateWorkOrderMRIHandler> _logger;
    private readonly CreateWorkOrderAtomicHandler _atomicHandler;
    private readonly IOptions<AppConfigs> _appConfigs;

    public CreateWorkOrderMRIHandler(
        ILogger<CreateWorkOrderMRIHandler> logger,
        CreateWorkOrderAtomicHandler atomicHandler,
        IOptions<AppConfigs> appConfigs)
    {
        _logger = logger;
        _atomicHandler = atomicHandler;
        _appConfigs = appConfigs;
    }

    /// <summary>
    /// Processes the Create Work Order request.
    /// </summary>
    /// <param name="request">The API request DTO.</param>
    /// <returns>The API response DTO.</returns>
    public async Task<BaseResponseDTO> ProcessRequest(CreateWorkOrderRequestDTO request)
    {
        _logger.Info(InfoConstants.PROCESSING_STARTED, "CreateWorkOrder");

        try
        {
            // Validate session
            ValidateSession(request.SessionId);

            // Map API request to downstream request
            var downstreamRequest = MapToDownstreamRequest(request);

            // Call atomic handler
            var downstreamResponse = await _atomicHandler.ExecuteAsync(
                request.SessionId!,
                downstreamRequest);

            // Map downstream response to API response
            var response = MapToApiResponse(downstreamResponse);

            _logger.Info(InfoConstants.PROCESSING_COMPLETED, "CreateWorkOrder");
            return response;
        }
        catch (DownStreamApiFailureException)
        {
            throw; // Re-throw downstream exceptions
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing CreateWorkOrder request.");
            throw new BaseException(ErrorConstants.CREATE_WORKORDER_FAILED, ex)
            {
                ErrorProperties = [ErrorConstants.CREATE_WORKORDER_FAILED]
            };
        }
    }

    private void ValidateSession(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new BaseException(ErrorConstants.SESSION_ID_NOT_FOUND_IN_CONTEXT)
            {
                ErrorProperties = [ErrorConstants.MRI_AUTHENTICATION_REQUIRED]
            };
        }
    }

    private CreateWorkOrderApiRequestDTO MapToDownstreamRequest(CreateWorkOrderRequestDTO request)
    {
        return new CreateWorkOrderApiRequestDTO
        {
            ContractId = _appConfigs.Value.MRI.ContractId,
            Description = request.Description,
            Priority = request.Priority,
            LocationId = request.LocationId,
            AssetId = request.AssetId,
            RequestedBy = request.RequestedBy,
            RequestedDate = request.RequestedDate,
            DueDate = request.DueDate,
            WorkOrderType = request.WorkOrderType,
            CategoryId = request.CategoryId,
            SubCategoryId = request.SubCategoryId,
            AssignedTo = request.AssignedTo,
            Notes = request.Notes,
            ExternalReference = request.ExternalReference
        };
    }

    private CreateWorkOrderResponseDTO MapToApiResponse(CreateWorkOrderApiResponseDTO downstreamResponse)
    {
        var result = downstreamResponse.Result;

        if (result == null || !result.Success)
        {
            return new CreateWorkOrderResponseDTO
            {
                Success = false,
                Message = result?.ErrorMessage ?? ErrorConstants.CREATE_WORKORDER_FAILED,
                ErrorProperties = [result?.ErrorCode ?? ErrorConstants.CREATE_WORKORDER_FAILED]
            };
        }

        return new CreateWorkOrderResponseDTO
        {
            Success = true,
            Message = result.Message ?? "Work order created successfully.",
            WorkOrder = new WorkOrderData
            {
                WorkOrderId = result.WorkOrderId,
                WorkOrderNumber = result.WorkOrderNumber,
                Status = result.Status,
                CreatedDate = result.CreatedDate,
                CreatedBy = result.CreatedBy
            }
        };
    }
}

using Microsoft.Extensions.Logging;
using SystemLayer.Application.DTOs;
using SystemLayer.Infrastructure.Models;

namespace SystemLayer.Infrastructure.Mapping;

/// <summary>
/// Service for mapping between System Layer DTOs and CAFM models
/// </summary>
public class CafmMappingService
{
    private readonly ILogger<CafmMappingService> _logger;
    
    public CafmMappingService(ILogger<CafmMappingService> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Maps CreateWorkOrderRequest to CAFM model
    /// </summary>
    public virtual CafmCreateWorkOrderRequest MapToCreateWorkOrderRequest(CreateWorkOrderRequest request, string sessionToken, string correlationId)
    {
        try
        {
            var cafmRequest = new CafmCreateWorkOrderRequest
            {
                SessionToken = sessionToken,
                WorkOrderNumber = request.WorkOrderNumber,
                Description = request.Description,
                LocationId = request.LocationId,
                Priority = request.Priority,
                AssignedTo = request.AssignedTo,
                ScheduledDate = request.ScheduledDate,
                InstructionSetId = request.InstructionSetId
            };
            
            // Map additional properties to custom fields
            foreach (var prop in request.AdditionalProperties)
            {
                cafmRequest.CustomFields.Add(new CafmCustomField
                {
                    Name = prop.Key,
                    Value = prop.Value?.ToString() ?? string.Empty
                });
            }
            
            _logger.LogDebug("Mapped CreateWorkOrderRequest to CAFM model with correlation {CorrelationId}", correlationId);
            return cafmRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map CreateWorkOrderRequest with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Maps CAFM CreateWorkOrderResponse to System Layer DTO
    /// </summary>
    public virtual CreateWorkOrderResponse MapFromCreateWorkOrderResponse(CafmCreateWorkOrderResponse cafmResponse, string correlationId)
    {
        try
        {
            var response = new CreateWorkOrderResponse
            {
                Success = cafmResponse.Success,
                WorkOrderId = cafmResponse.WorkOrderId,
                WorkOrderNumber = cafmResponse.WorkOrderNumber,
                Message = cafmResponse.Message
            };
            
            // Add error code as additional data if present
            if (!string.IsNullOrEmpty(cafmResponse.ErrorCode))
            {
                response.AdditionalData["ErrorCode"] = cafmResponse.ErrorCode;
            }
            
            _logger.LogDebug("Mapped CAFM CreateWorkOrderResponse to System Layer DTO with correlation {CorrelationId}", correlationId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map CreateWorkOrderResponse with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Maps GetLocationRequest to CAFM model
    /// </summary>
    public virtual CafmGetLocationRequest MapToGetLocationRequest(GetLocationRequest request, string sessionToken, string correlationId)
    {
        try
        {
            var cafmRequest = new CafmGetLocationRequest
            {
                SessionToken = sessionToken,
                LocationId = request.LocationId,
                IncludeHierarchy = request.IncludeHierarchy
            };
            
            _logger.LogDebug("Mapped GetLocationRequest to CAFM model with correlation {CorrelationId}", correlationId);
            return cafmRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map GetLocationRequest with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Maps CAFM GetLocationResponse to System Layer DTO
    /// </summary>
    public virtual LocationResponse MapFromGetLocationResponse(CafmGetLocationResponse cafmResponse, string correlationId)
    {
        try
        {
            if (cafmResponse.Location == null)
            {
                throw new InvalidOperationException("CAFM location response contains no location data");
            }
            
            var response = new LocationResponse
            {
                LocationId = cafmResponse.Location.LocationId,
                LocationName = cafmResponse.Location.LocationName,
                ParentLocationId = cafmResponse.Location.ParentLocationId,
                LocationType = cafmResponse.Location.LocationType,
                Status = cafmResponse.Location.Status
            };
            
            // Map properties
            foreach (var prop in cafmResponse.Location.Properties)
            {
                response.Properties[prop.Name] = prop.Value;
            }
            
            _logger.LogDebug("Mapped CAFM GetLocationResponse to System Layer DTO with correlation {CorrelationId}", correlationId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map GetLocationResponse with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Maps GetBreakdownTaskRequest to CAFM model
    /// </summary>
    public virtual CafmGetBreakdownTaskRequest MapToGetBreakdownTaskRequest(GetBreakdownTaskRequest request, string sessionToken, string correlationId)
    {
        try
        {
            var cafmRequest = new CafmGetBreakdownTaskRequest
            {
                SessionToken = sessionToken,
                TaskId = request.TaskId,
                IncludeDetails = request.IncludeDetails
            };
            
            _logger.LogDebug("Mapped GetBreakdownTaskRequest to CAFM model with correlation {CorrelationId}", correlationId);
            return cafmRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map GetBreakdownTaskRequest with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Maps CAFM GetBreakdownTaskResponse to System Layer DTO
    /// </summary>
    public virtual BreakdownTaskResponse MapFromGetBreakdownTaskResponse(CafmGetBreakdownTaskResponse cafmResponse, string correlationId)
    {
        try
        {
            if (cafmResponse.Task == null)
            {
                throw new InvalidOperationException("CAFM breakdown task response contains no task data");
            }
            
            var response = new BreakdownTaskResponse
            {
                TaskId = cafmResponse.Task.TaskId,
                TaskName = cafmResponse.Task.TaskName,
                Description = cafmResponse.Task.Description,
                Status = cafmResponse.Task.Status,
                RequiredSkills = cafmResponse.Task.RequiredSkills
            };
            
            // Convert estimated duration from minutes to TimeSpan
            if (cafmResponse.Task.EstimatedDurationMinutes.HasValue)
            {
                response.EstimatedDuration = TimeSpan.FromMinutes(cafmResponse.Task.EstimatedDurationMinutes.Value);
            }
            
            // Map task details
            foreach (var detail in cafmResponse.Task.TaskDetails)
            {
                response.TaskDetails[detail.Name] = detail.Value;
            }
            
            _logger.LogDebug("Mapped CAFM GetBreakdownTaskResponse to System Layer DTO with correlation {CorrelationId}", correlationId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map GetBreakdownTaskResponse with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Maps GetInstructionSetsRequest to CAFM model
    /// </summary>
    public virtual CafmGetInstructionSetsRequest MapToGetInstructionSetsRequest(GetInstructionSetsRequest request, string sessionToken, string correlationId)
    {
        try
        {
            var cafmRequest = new CafmGetInstructionSetsRequest
            {
                SessionToken = sessionToken,
                CategoryFilter = request.CategoryFilter,
                AssetTypeFilter = request.AssetTypeFilter,
                MaxResults = request.MaxResults
            };
            
            _logger.LogDebug("Mapped GetInstructionSetsRequest to CAFM model with correlation {CorrelationId}", correlationId);
            return cafmRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map GetInstructionSetsRequest with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
    
    /// <summary>
    /// Maps CAFM GetInstructionSetsResponse to System Layer DTO
    /// </summary>
    public virtual GetInstructionSetsResponse MapFromGetInstructionSetsResponse(CafmGetInstructionSetsResponse cafmResponse, string correlationId)
    {
        try
        {
            var response = new GetInstructionSetsResponse
            {
                TotalCount = cafmResponse.TotalCount,
                HasMore = cafmResponse.HasMore
            };
            
            // Map instruction sets
            foreach (var cafmInstructionSet in cafmResponse.InstructionSets)
            {
                var instructionSet = new InstructionSetResponse
                {
                    InstructionSetId = cafmInstructionSet.InstructionSetId,
                    Name = cafmInstructionSet.Name,
                    Category = cafmInstructionSet.Category,
                    AssetType = cafmInstructionSet.AssetType
                };
                
                // Map steps
                foreach (var cafmStep in cafmInstructionSet.Steps)
                {
                    var step = new InstructionStep
                    {
                        StepNumber = cafmStep.StepNumber,
                        Description = cafmStep.Description,
                        RequiredTools = cafmStep.RequiredTools
                    };
                    
                    // Convert estimated duration from minutes to TimeSpan
                    if (cafmStep.EstimatedDurationMinutes.HasValue)
                    {
                        step.EstimatedDuration = TimeSpan.FromMinutes(cafmStep.EstimatedDurationMinutes.Value);
                    }
                    
                    // Map step data
                    foreach (var stepData in cafmStep.StepData)
                    {
                        step.StepData[stepData.Name] = stepData.Value;
                    }
                    
                    instructionSet.Steps.Add(step);
                }
                
                // Map metadata
                foreach (var metadata in cafmInstructionSet.Metadata)
                {
                    instructionSet.Metadata[metadata.Name] = metadata.Value;
                }
                
                response.InstructionSets.Add(instructionSet);
            }
            
            _logger.LogDebug("Mapped CAFM GetInstructionSetsResponse to System Layer DTO with correlation {CorrelationId}", correlationId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to map GetInstructionSetsResponse with correlation {CorrelationId}", correlationId);
            throw;
        }
    }
}
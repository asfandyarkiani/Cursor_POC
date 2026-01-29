using Core.DTOs;
using Core.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using sys_cafm_workorder.Abstractions;
using sys_cafm_workorder.DTOs.Api.CAFM;
using System.Net;

namespace sys_cafm_workorder.Functions.CAFM;

/// <summary>
/// Azure Function for getting locations from CAFM.
/// POST /api/cafm/locations
/// </summary>
public class CAFMGetLocationsFunction
{
    private readonly ICAFMWorkOrderService _workOrderService;
    private readonly ILogger<CAFMGetLocationsFunction> _logger;

    public CAFMGetLocationsFunction(
        ICAFMWorkOrderService workOrderService,
        ILogger<CAFMGetLocationsFunction> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [Function("CAFMGetLocations")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/locations")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.Info("CAFMGetLocations function triggered");

        // Store HttpResponseData in context for middleware
        var response = req.CreateResponse(HttpStatusCode.OK);
        context.Items[typeof(HttpResponseData).Name] = response;

        // Read request body
        var requestDto = await req.ReadBodyAsync<GetLocationsRequestDto>();

        if (requestDto == null)
        {
            throw new Core.Exceptions.NoRequestBodyException(
                null,
                new List<string> { "Request body is required" },
                nameof(CAFMGetLocationsFunction));
        }

        // Call service
        return await _workOrderService.GetLocationsAsync(requestDto);
    }
}

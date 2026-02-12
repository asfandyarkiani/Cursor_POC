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
/// Azure Function for creating an event in CAFM.
/// POST /api/cafm/events/create
/// </summary>
public class CAFMCreateEventFunction
{
    private readonly ICAFMWorkOrderService _workOrderService;
    private readonly ILogger<CAFMCreateEventFunction> _logger;

    public CAFMCreateEventFunction(
        ICAFMWorkOrderService workOrderService,
        ILogger<CAFMCreateEventFunction> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [Function("CAFMCreateEvent")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/events/create")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.Info("CAFMCreateEvent function triggered");

        // Store HttpResponseData in context for middleware
        var response = req.CreateResponse(HttpStatusCode.OK);
        context.Items[typeof(HttpResponseData).Name] = response;

        // Read request body
        var requestDto = await req.ReadBodyAsync<CreateEventRequestDto>();

        if (requestDto == null)
        {
            throw new Core.Exceptions.NoRequestBodyException(
                null,
                new List<string> { "Request body is required" },
                nameof(CAFMCreateEventFunction));
        }

        // Call service
        return await _workOrderService.CreateEventAsync(requestDto);
    }
}

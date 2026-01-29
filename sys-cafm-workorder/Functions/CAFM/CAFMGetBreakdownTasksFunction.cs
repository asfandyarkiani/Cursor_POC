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
/// Azure Function for getting breakdown tasks from CAFM.
/// POST /api/cafm/breakdowntasks
/// </summary>
public class CAFMGetBreakdownTasksFunction
{
    private readonly ICAFMWorkOrderService _workOrderService;
    private readonly ILogger<CAFMGetBreakdownTasksFunction> _logger;

    public CAFMGetBreakdownTasksFunction(
        ICAFMWorkOrderService workOrderService,
        ILogger<CAFMGetBreakdownTasksFunction> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [Function("CAFMGetBreakdownTasks")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/breakdowntasks")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.Info("CAFMGetBreakdownTasks function triggered");

        // Store HttpResponseData in context for middleware
        var response = req.CreateResponse(HttpStatusCode.OK);
        context.Items[typeof(HttpResponseData).Name] = response;

        // Read request body
        var requestDto = await req.ReadBodyAsync<GetBreakdownTasksRequestDto>();

        if (requestDto == null)
        {
            throw new Core.Exceptions.NoRequestBodyException(
                null,
                new List<string> { "Request body is required" },
                nameof(CAFMGetBreakdownTasksFunction));
        }

        // Call service
        return await _workOrderService.GetBreakdownTasksAsync(requestDto);
    }
}

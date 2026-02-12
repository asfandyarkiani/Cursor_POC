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
/// Azure Function for creating a breakdown task in CAFM.
/// POST /api/cafm/breakdowntasks/create
/// </summary>
public class CAFMCreateBreakdownTaskFunction
{
    private readonly ICAFMWorkOrderService _workOrderService;
    private readonly ILogger<CAFMCreateBreakdownTaskFunction> _logger;

    public CAFMCreateBreakdownTaskFunction(
        ICAFMWorkOrderService workOrderService,
        ILogger<CAFMCreateBreakdownTaskFunction> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [Function("CAFMCreateBreakdownTask")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/breakdowntasks/create")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.Info("CAFMCreateBreakdownTask function triggered");

        // Store HttpResponseData in context for middleware
        var response = req.CreateResponse(HttpStatusCode.OK);
        context.Items[typeof(HttpResponseData).Name] = response;

        // Read request body
        var requestDto = await req.ReadBodyAsync<CreateBreakdownTaskRequestDto>();

        if (requestDto == null)
        {
            throw new Core.Exceptions.NoRequestBodyException(
                null,
                new List<string> { "Request body is required" },
                nameof(CAFMCreateBreakdownTaskFunction));
        }

        // Call service
        return await _workOrderService.CreateBreakdownTaskAsync(requestDto);
    }
}

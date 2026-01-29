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
/// Azure Function for getting instruction sets from CAFM.
/// POST /api/cafm/instructionsets
/// </summary>
public class CAFMGetInstructionSetsFunction
{
    private readonly ICAFMWorkOrderService _workOrderService;
    private readonly ILogger<CAFMGetInstructionSetsFunction> _logger;

    public CAFMGetInstructionSetsFunction(
        ICAFMWorkOrderService workOrderService,
        ILogger<CAFMGetInstructionSetsFunction> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [Function("CAFMGetInstructionSets")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/instructionsets")] HttpRequestData req,
        FunctionContext context)
    {
        _logger.Info("CAFMGetInstructionSets function triggered");

        // Store HttpResponseData in context for middleware
        var response = req.CreateResponse(HttpStatusCode.OK);
        context.Items[typeof(HttpResponseData).Name] = response;

        // Read request body
        var requestDto = await req.ReadBodyAsync<GetInstructionSetsRequestDto>();

        if (requestDto == null)
        {
            throw new Core.Exceptions.NoRequestBodyException(
                null,
                new List<string> { "Request body is required" },
                nameof(CAFMGetInstructionSetsFunction));
        }

        // Call service
        return await _workOrderService.GetInstructionSetsAsync(requestDto);
    }
}

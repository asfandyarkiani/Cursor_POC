using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FacilitiesMgmtSystem.Attributes;
using FacilitiesMgmtSystem.Constants;
using FacilitiesMgmtSystem.Core.Exceptions;
using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.DTO.CreateWorkOrderDTO;
using FacilitiesMgmtSystem.Helper;
using FacilitiesMgmtSystem.Implementations.MRI.Handlers;

namespace FacilitiesMgmtSystem.Functions;

/// <summary>
/// Azure Function for Create Work Order operation.
/// </summary>
public class CreateWorkOrderAPI
{
    private readonly ILogger<CreateWorkOrderAPI> _logger;
    private readonly CreateWorkOrderMRIHandler _handler;

    public CreateWorkOrderAPI(
        ILogger<CreateWorkOrderAPI> logger,
        CreateWorkOrderMRIHandler handler)
    {
        _logger = logger;
        _handler = handler;
    }

    /// <summary>
    /// HTTP-triggered function to create a work order in CAFM/MRI.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <param name="context">The function execution context.</param>
    /// <returns>A BaseResponseDTO containing the operation result.</returns>
    [MRIAuthentication]
    [Function("CreateWorkOrder")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workorder/create")] HttpRequest req,
        FunctionContext context)
    {
        _logger.Info(InfoConstants.REQUEST_RECEIVED, "CreateWorkOrderAPI");

        // Read and validate request body
        var request = await req.ReadBodyAsync<CreateWorkOrderRequestDTO>();
        if (request == null)
        {
            _logger.Warn("Invalid or null request payload received.");
            throw new RequestValidationFailureException()
            {
                ErrorProperties = [ErrorConstants.INVALID_REQ_PAYLOAD]
            };
        }

        // Validate request parameters
        request.ValidateAPIRequestParameters();
        _logger.Info(InfoConstants.REQUEST_VALIDATED);

        // Extract session ID from context (populated by MRIAuthenticationMiddleware)
        if (context.Items.TryGetValue(InfoConstants.SESSION_ID, out var sessionIdObj) &&
            sessionIdObj is string sessionId)
        {
            request.SessionId = sessionId;
        }
        else
        {
            _logger.Error("Session ID not found in function context.");
            throw new BaseException(ErrorConstants.SESSION_ID_NOT_FOUND_IN_CONTEXT)
            {
                ErrorProperties = [ErrorConstants.MRI_AUTHENTICATION_REQUIRED]
            };
        }

        // Process the request through the handler
        var result = await _handler.ProcessRequest(request);
        return result;
    }
}

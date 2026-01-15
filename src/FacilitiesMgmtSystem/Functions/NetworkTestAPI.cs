using FacilitiesMgmtSystem.DTO;
using FacilitiesMgmtSystem.DTO.NetworkTestDTO;
using FacilitiesMgmtSystem.Helper;
using FacilitiesMgmtSystem.Implementations.MRI.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FacilitiesMgmtSystem.Functions;

/// <summary>
/// Azure Function for Network Test / Health Check endpoint.
/// This endpoint verifies network connectivity and returns a success message.
/// Based on Boomi "Network Test" process configuration.
/// </summary>
public class NetworkTestAPI
{
    private readonly ILogger<NetworkTestAPI> _logger;
    private readonly NetworkTestMRIHandler _handler;

    public NetworkTestAPI(
        ILogger<NetworkTestAPI> logger,
        NetworkTestMRIHandler handler)
    {
        _logger = logger;
        _handler = handler;
    }

    /// <summary>
    /// Executes the network test.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <param name="context">The function context.</param>
    /// <returns>A BaseResponseDTO with the test result.</returns>
    [Function("NetworkTest")]
    public async Task<BaseResponseDTO> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
        FunctionContext context)
    {
        _logger.Info("HTTP received for NetworkTestAPI.");

        // Network Test accepts both GET and POST with optional body
        NetworkTestRequestDTO? request = null;
        
        if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
            req.ContentLength > 0)
        {
            request = await req.ReadBodyAsync<NetworkTestRequestDTO>();
        }

        var result = await _handler.ProcessRequest(request);
        return result;
    }
}

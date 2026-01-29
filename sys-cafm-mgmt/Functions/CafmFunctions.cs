using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SysCafmMgmt.Abstractions;
using SysCafmMgmt.DTOs.API;
using System.Net;
using System.Text.Json;
using Core.Extensions;

namespace SysCafmMgmt.Functions
{
    /// <summary>
    /// Azure Functions for CAFM (Computer-Aided Facility Management) operations
    /// Exposes HTTP endpoints for System Layer operations to interact with FSI CAFM
    /// </summary>
    public class CafmFunctions
    {
        private readonly ICafmMgmt _cafmMgmt;
        private readonly ILogger<CafmFunctions> _logger;

        public CafmFunctions(ICafmMgmt cafmMgmt, ILogger<CafmFunctions> logger)
        {
            _cafmMgmt = cafmMgmt ?? throw new ArgumentNullException(nameof(cafmMgmt));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// HTTP endpoint for creating a work order in CAFM
        /// POST /api/cafm/workorders
        /// </summary>
        [Function("CreateWorkOrder")]
        public async Task<HttpResponseData> CreateWorkOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/workorders")] HttpRequestData req)
        {
            _logger.Info("[CreateWorkOrder] Function invoked");

            try
            {
                // Parse request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<CreateWorkOrderRequestDTO>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                {
                    _logger.Warning("[CreateWorkOrder] Invalid request body");
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new { Error = "Invalid request body" });
                    return badRequestResponse;
                }

                // Call service
                var result = await _cafmMgmt.CreateWorkOrderAsync(request);

                // Create response
                var response = req.CreateResponse((HttpStatusCode)result.StatusCode);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CreateWorkOrder] Unhandled exception");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = ex.Message });
                return errorResponse;
            }
        }

        /// <summary>
        /// HTTP endpoint for getting locations from CAFM
        /// POST /api/cafm/locations
        /// </summary>
        [Function("GetLocations")]
        public async Task<HttpResponseData> GetLocations(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/locations")] HttpRequestData req)
        {
            _logger.Info("[GetLocations] Function invoked");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<GetLocationsRequestDTO>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new { Error = "Invalid request body" });
                    return badRequestResponse;
                }

                var result = await _cafmMgmt.GetLocationsAsync(request);

                var response = req.CreateResponse((HttpStatusCode)result.StatusCode);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[GetLocations] Unhandled exception");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = ex.Message });
                return errorResponse;
            }
        }

        /// <summary>
        /// HTTP endpoint for getting instruction sets from CAFM
        /// POST /api/cafm/instructionsets
        /// </summary>
        [Function("GetInstructionSets")]
        public async Task<HttpResponseData> GetInstructionSets(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/instructionsets")] HttpRequestData req)
        {
            _logger.Info("[GetInstructionSets] Function invoked");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<GetInstructionSetsRequestDTO>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new { Error = "Invalid request body" });
                    return badRequestResponse;
                }

                var result = await _cafmMgmt.GetInstructionSetsAsync(request);

                var response = req.CreateResponse((HttpStatusCode)result.StatusCode);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[GetInstructionSets] Unhandled exception");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = ex.Message });
                return errorResponse;
            }
        }

        /// <summary>
        /// HTTP endpoint for getting breakdown tasks from CAFM
        /// POST /api/cafm/breakdowntasks
        /// </summary>
        [Function("GetBreakdownTasks")]
        public async Task<HttpResponseData> GetBreakdownTasks(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/breakdowntasks")] HttpRequestData req)
        {
            _logger.Info("[GetBreakdownTasks] Function invoked");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<GetBreakdownTasksRequestDTO>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new { Error = "Invalid request body" });
                    return badRequestResponse;
                }

                var result = await _cafmMgmt.GetBreakdownTasksAsync(request);

                var response = req.CreateResponse((HttpStatusCode)result.StatusCode);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[GetBreakdownTasks] Unhandled exception");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = ex.Message });
                return errorResponse;
            }
        }

        /// <summary>
        /// HTTP endpoint for creating an event in CAFM
        /// POST /api/cafm/events
        /// </summary>
        [Function("CreateEvent")]
        public async Task<HttpResponseData> CreateEvent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/events")] HttpRequestData req)
        {
            _logger.Info("[CreateEvent] Function invoked");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<CreateEventRequestDTO>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (request == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new { Error = "Invalid request body" });
                    return badRequestResponse;
                }

                var result = await _cafmMgmt.CreateEventAsync(request);

                var response = req.CreateResponse((HttpStatusCode)result.StatusCode);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CreateEvent] Unhandled exception");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = ex.Message });
                return errorResponse;
            }
        }

        /// <summary>
        /// HTTP endpoint for CAFM login
        /// POST /api/cafm/login
        /// </summary>
        [Function("CafmLogin")]
        public async Task<HttpResponseData> CafmLogin(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/login")] HttpRequestData req)
        {
            _logger.Info("[CafmLogin] Function invoked");

            try
            {
                var result = await _cafmMgmt.LoginAsync();

                var response = req.CreateResponse((HttpStatusCode)result.StatusCode);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CafmLogin] Unhandled exception");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = ex.Message });
                return errorResponse;
            }
        }

        /// <summary>
        /// HTTP endpoint for CAFM logout
        /// POST /api/cafm/logout
        /// </summary>
        [Function("CafmLogout")]
        public async Task<HttpResponseData> CafmLogout(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cafm/logout")] HttpRequestData req)
        {
            _logger.Info("[CafmLogout] Function invoked");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                string? sessionId = request?.GetValueOrDefault("sessionId");

                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new { Error = "SessionId is required" });
                    return badRequestResponse;
                }

                var result = await _cafmMgmt.LogoutAsync(sessionId);

                var response = req.CreateResponse((HttpStatusCode)result.StatusCode);
                await response.WriteAsJsonAsync(result);
                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "[CafmLogout] Unhandled exception");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { Error = ex.Message });
                return errorResponse;
            }
        }
    }
}

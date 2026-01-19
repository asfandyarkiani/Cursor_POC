using System.Diagnostics;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using FacilitiesMgmtSystem.Helper;

namespace FacilitiesMgmtSystem.Middlewares;

/// <summary>
/// Middleware that tracks function execution time for performance monitoring.
/// </summary>
public class ExecutionTimingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExecutionTimingMiddleware> _logger;

    public ExecutionTimingMiddleware(ILogger<ExecutionTimingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var functionName = context.FunctionDefinition.Name;
        var stopwatch = Stopwatch.StartNew();
        
        _logger.Info("Function {FunctionName} execution started.", functionName);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.Info("Function {FunctionName} execution completed in {ElapsedMs}ms.", 
                functionName, stopwatch.ElapsedMilliseconds);
        }
    }
}

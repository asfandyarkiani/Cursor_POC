using System.Diagnostics;
using FacilitiesMgmtSystem.Helper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace FacilitiesMgmtSystem.Middlewares;

/// <summary>
/// Middleware to track execution time of function invocations.
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
        
        _logger.Info("Function {FunctionName} started.", functionName);
        
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.Info("Function {FunctionName} completed in {ElapsedMs}ms.", 
                functionName, stopwatch.ElapsedMilliseconds);
        }
    }
}

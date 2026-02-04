using Microsoft.Extensions.Logging;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Extension methods for ILogger to provide consistent logging patterns.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public static void Info(this ILogger logger, string message, params object?[] args)
    {
        logger.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public static void Warn(this ILogger logger, string message, params object?[] args)
    {
        logger.LogWarning(message, args);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public static void Error(this ILogger logger, string message, params object?[] args)
    {
        logger.LogError(message, args);
    }

    /// <summary>
    /// Logs an error message with exception.
    /// </summary>
    public static void Error(this ILogger logger, Exception exception, string message, params object?[] args)
    {
        logger.LogError(exception, message, args);
    }
}

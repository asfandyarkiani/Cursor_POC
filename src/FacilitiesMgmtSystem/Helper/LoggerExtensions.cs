using Microsoft.Extensions.Logging;

namespace FacilitiesMgmtSystem.Helper;

/// <summary>
/// Extension methods for ILogger to provide consistent logging patterns.
/// </summary>
public static class LoggerExtensions
{
    public static void Info(this ILogger logger, string message, params object[] args)
    {
        logger.LogInformation(message, args);
    }

    public static void Warn(this ILogger logger, string message, params object[] args)
    {
        logger.LogWarning(message, args);
    }

    public static void Error(this ILogger logger, Exception exception, string message, params object[] args)
    {
        logger.LogError(exception, message, args);
    }

    public static void Error(this ILogger logger, string message, params object[] args)
    {
        logger.LogError(message, args);
    }

    public static void Debug(this ILogger logger, string message, params object[] args)
    {
        logger.LogDebug(message, args);
    }
}

using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Core.Extensions
{
    public static class LoggerExtensions
    {
        public static void Info<T>(this ILogger<T> log, string message, [CallerMemberName] string function = "") =>
            log.LogInformation("[{Class}][{Function}] {Message}", typeof(T).Name, function, message);

        public static void Warn<T>(this ILogger<T> log, string message, [CallerMemberName] string function = "") =>
            log.LogWarning("[{Class}][{Function}] {Message}", typeof(T).Name, function, message);

        public static void Error<T>(this ILogger<T> log, string message, [CallerMemberName] string function = "") =>
            log.LogError("[{Class}][{Function}] {Message}", typeof(T).Name, function, message);

        public static void Error<T>(this ILogger<T> log, Exception ex, string message, [CallerMemberName] string function = "") =>
            log.LogError(ex, "[{Class}][{Function}] {Message}", typeof(T).Name, function, message);

        public static void Debug<T>(this ILogger<T> log, string message, [CallerMemberName] string function = "") =>
            log.LogDebug("[{Class}][{Function}] {Message}", typeof(T).Name, function, message);

        public static void Trace<T>(this ILogger<T> log, string message, [CallerMemberName] string function = "") =>
            log.LogTrace("[{Class}][{Function}] {Message}", typeof(T).Name, function, message);

        public static void Critical<T>(this ILogger<T> log, string message, [CallerMemberName] string function = "") =>
                log.LogCritical("[{Class}][{Function}] {Message}", typeof(T).Name, function, message);

        public static void Critical<T>(this ILogger<T> log, Exception ex, string message, [CallerMemberName] string function = "") =>
            log.LogCritical(ex, "[{Class}][{Function}] {Message}", typeof(T).Name, function, message);
    }
}

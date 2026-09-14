using System.Diagnostics;
using StardewModdingAPI;

namespace SpiderCore.Common.Logging;

public static class Log
{
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // ReSharper disable once MemberCanBePrivate.Global
    internal static IMonitor Monitor = null!;
    
    [Conditional("DEBUG")]
    public static void Debug<T>(T message) => Monitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Debug);

    public static void Error<T>(T message) => Monitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Error);

    public static void Warn<T>(T message) => Monitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Warn);

    public static void Info<T>(T message) => Monitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Info);

    public static void Trace<T>(T message) => Monitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Trace);

    public static void Alert<T>(T message) => Monitor.Log(
        $"{(message is not string ? "[" + message?.GetType() + "] " : string.Empty)}{message?.ToString() ?? string.Empty}",
        LogLevel.Alert);
}
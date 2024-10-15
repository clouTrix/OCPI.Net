using Microsoft.Extensions.Logging;

namespace OCPI;

public class LazyLogging(ILogger? logger = null)
{
    public ILogger? Underlying => logger;
    private static Action<string, Func<object?[]>> DefaultNOOP = (_, _) => { };

    public Action<string, Func<object?[]>> LogTrace    => Log(LogLevel.Trace);
    public Action<string, Func<object?[]>> LogDebug    => Log(LogLevel.Debug);
    public Action<string, Func<object?[]>> LogInfo     => Log(LogLevel.Information);
    public Action<string, Func<object?[]>> LogWarn     => Log(LogLevel.Warning);
    public Action<string, Func<object?[]>> LogError    => Log(LogLevel.Error);
    public Action<string, Func<object?[]>> LogCritical => Log(LogLevel.Critical);

    public Action<string, Func<object?[]>> Log(LogLevel level)
        => logger?.IsEnabled(level) ?? false
                        ? (message, parameters) => logger.Log(level, message, parameters.Invoke())
                        : DefaultNOOP;
}
using Microsoft.Extensions.Logging;

namespace OCPI;

public class LazyLogging(ILogger? logger = null): ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => logger?.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel)
        => logger?.IsEnabled(logLevel) ?? false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => logger?.Log(logLevel, eventId, state, exception, formatter);

    public void Trace(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Trace)(message, paramsProvider);

    public void Debug(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Debug)(message, paramsProvider);

    public void Info(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Information)(message, paramsProvider);

    public void Warn(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Warning)(message, paramsProvider);

    public void Error(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Error)(message, paramsProvider);

    public void Critical(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Critical)(message, paramsProvider);

    private static Action<string, Func<object?[]>> DefaultNOOP = (_, _) => { };

    public Action<string, Func<object?[]>> Log(LogLevel level)
        => logger?.IsEnabled(level) ?? false
                        ? (message, parameters) => logger!.Log(level, message, parameters.Invoke())
                        : DefaultNOOP;
}

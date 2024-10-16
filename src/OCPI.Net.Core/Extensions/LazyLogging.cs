using Microsoft.Extensions.Logging;

namespace OCPI;

public class LazyLogging(ILogger? logger = null): ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel)
        => logger?.IsEnabled(logLevel) ?? false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => logger?.Log(logLevel, eventId, state, exception, formatter);

    public void Trace(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Trace)(message, paramsProvider);

    public void Trace(string message, params object?[] parameters)
        => Log(LogLevel.Trace, message, parameters);

    public void Debug(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Debug)(message, paramsProvider);

    public void Debug(string message, params object?[] parameters)
        => Log(LogLevel.Debug, message, parameters);
    
    public void Info(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Information)(message, paramsProvider);

    public void Info(string message, params object?[] parameters)
        => Log(LogLevel.Information, message, parameters);

    public void Warn(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Warning)(message, paramsProvider);

    public void Warn(string message, params object?[] parameters)
        => Log(LogLevel.Warning, message, parameters);

    public void Error(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Error)(message, paramsProvider);

    public void Error(string message, params object?[] parameters)
        => Log(LogLevel.Error, message, parameters);

    public void Critical(string message, Func<object?[]> paramsProvider)
        => Log(LogLevel.Critical)(message, paramsProvider);

    public void Critical(string message, params object?[] parameters)
        => Log(LogLevel.Critical, message, parameters);

    public void Log(LogLevel level, string message, params object?[] parameters)
    {
        if(logger?.IsEnabled(level) ?? false) logger.Log(level, message, parameters);
    }

    private static Action<string, Func<object?[]>> DefaultNOOP = (_, _) => { };

    public Action<string, Func<object?[]>> Log(LogLevel level)
        => logger?.IsEnabled(level) ?? false
                        ? (message, parameters) => logger.Log(level, message, parameters.Invoke())
                        : DefaultNOOP;
}
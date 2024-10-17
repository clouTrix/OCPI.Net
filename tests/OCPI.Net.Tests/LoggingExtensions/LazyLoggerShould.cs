using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Extensions.Logging;
using Xunit.Sdk;

namespace OCPI.Tests.LoggingExtensions;

public class LazyLoggerShould(ITestOutputHelper output)
{
    private ILoggerFactory FactoryFor(LogLevel rootLevel)
        => LoggerFactory.Create(
                    builder => builder.AddProvider(new XunitLoggerProvider(output, (_,level) => level >= rootLevel))  
                                              .SetMinimumLevel(LogLevel.Trace)
                            );

    private int paramOnHit()   => 666;
    private int paramOnNoHit() => throw FailException.ForFailure("should not be hit");

    [Fact]
    public void BehaveAs_ILogger()
    {
        var logger = new LazyLogging(FactoryFor(LogLevel.Trace).CreateLogger<LazyLoggerShould>());
        logger.LogTrace       ("my message: {M}", paramOnHit());
        logger.LogDebug       ("my message: {M}", paramOnHit());
        logger.LogInformation ("my message: {M}", paramOnHit());
        logger.LogWarning     ("my message: {M}", paramOnHit());
        logger.LogError       ("my message: {M}", paramOnHit());
        logger.LogCritical    ("my message: {M}", paramOnHit());
        
        logger.Log(LogLevel.Information, "my message: {M}", paramOnHit());
    }

    
    [Fact]
    public void NotMaterializeParameters_When_NoLogger()
    {
        var logger = new LazyLogging();
        logger.Trace   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.Debug   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.Info    ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.Warn    ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.Error   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.Critical("my message: {M}", () => [ paramOnNoHit() ]);
    }

    [Fact]
    public void NotMaterializeParameters_When_NotLogging()
    {
        var logger = new LazyLogging(FactoryFor(LogLevel.Warning).CreateLogger<LazyLoggerShould>());
        logger.Trace   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.Debug   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.Info    ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.Warn    ("my message: {M}", () => [ paramOnHit() ]);
        logger.Error   ("my message: {M}", () => [ paramOnHit() ]);
        logger.Critical("my message: {M}", () => [ paramOnHit() ]);
    }

    [Fact]
    public void MaterializeParameters_When_Logging()
    {
        var logger = new LazyLogging(FactoryFor(LogLevel.Trace).CreateLogger<LazyLoggerShould>());
        logger.Trace   ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.Debug   ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.Info    ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.Warn    ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.Error   ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.Critical("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
    }

    [Fact]
    public void Support_Scopes()
    {
        var logger = new LazyLogging(FactoryFor(LogLevel.Information).CreateLogger<LazyLoggerShould>());
        using (logger.BeginScope("my scope"))
        {
            logger.Trace("my message: {M}/{A}", () => [ paramOnNoHit(), "blah" ]);
            logger.Info ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
            logger.LogWarning ("my message: {M}/{A}", paramOnHit(), "blah");
        }
    }
}
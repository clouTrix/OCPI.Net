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
    public void NotMaterializeParameters_When_NoLogger()
    {
        var logger = new LazyLogging();
        logger.LogTrace   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.LogDebug   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.LogWarn    ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.LogError   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.LogCritical("my message: {M}", () => [ paramOnNoHit() ]);
    }

    [Fact]
    public void NotMaterializeParameters_When_NotLogging()
    {
        var logger = new LazyLogging(FactoryFor(LogLevel.Warning).CreateLogger<LazyLoggerShould>());
        logger.LogTrace   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.LogDebug   ("my message: {M}", () => [ paramOnNoHit() ]);
        logger.LogWarn    ("my message: {M}", () => [ paramOnHit() ]);
        logger.LogError   ("my message: {M}", () => [ paramOnHit() ]);
        logger.LogCritical("my message: {M}", () => [ paramOnHit() ]);
    }

    [Fact]
    public void MaterializeParameters_When_Logging()
    {
        var logger = new LazyLogging(FactoryFor(LogLevel.Trace).CreateLogger<LazyLoggerShould>());
        logger.LogTrace   ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.LogDebug   ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.LogInfo    ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.LogWarn    ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.LogError   ("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
        logger.LogCritical("my message: {M}/{A}", () => [ paramOnHit(), "blah" ]);
    }
    
}
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OCPI.Contracts;
using OCPI.Validation;

namespace OCPI.Tests.Validation;

public class AddOcpiValidationTests
{
    [Fact]
    public void AddOcpiValidation_OnEmptyServiceCollection_AddsValidators()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddScoped(x => new OcpiValidationContext(ActionType.Get, OcpiVersion.v2_2_1));

        // Act
        services.AddOcpiValidation();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var ocpiValidator = serviceProvider.GetRequiredService<IOcpiValidator<OcpiCredentialsRole>>();
        
        Assert.Equal(ActionType.Get, ocpiValidator.ActionType);
        Assert.Equal(OcpiVersion.v2_2_1, ocpiValidator.OcpiVersion);
        
        Assert.NotNull(ocpiValidator);
    }
}

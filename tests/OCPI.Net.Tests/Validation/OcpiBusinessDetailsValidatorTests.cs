using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OCPI.Contracts;
using OCPI.Validation;

namespace OCPI.Tests.Validation;

public class OcpiBusinessDetailsValidatorTests
{
    [Fact]
    public void It_Should_BeAble_ToDo_StrictUrlValidation()
    {
        var validator = new ServiceCollection()
                                .AddScoped(x => new OcpiValidationContext(ActionType.Get, OcpiVersion.v2_1_1))
                                .AddOcpiValidation()
                                .BuildServiceProvider()
                                .GetRequiredService<IOcpiValidator<OcpiBusinessDetails>>();
        
        validator.Validate(new OcpiBusinessDetails { Name = "something", Website = "blah blah blah" }).IsValid.Should().BeFalse();
        validator.Validate(new OcpiBusinessDetails { Name = "something", Website = "www.example.com" }).IsValid.Should().BeFalse();
        validator.Validate(new OcpiBusinessDetails { Name = "something", Website = "http://www.example.com" }).IsValid.Should().BeTrue();
        validator.Validate(new OcpiBusinessDetails { Name = "something", Website = "https://www.example.com:8080/path" }).IsValid.Should().BeTrue();
    }

    [Fact]
    public void It_Should_BeAble_To_RelaxOn_UrlValidation()
    {
        var validator = new ServiceCollection()
            .AddScoped(x => new OcpiValidationContext(ActionType.Get, OcpiVersion.v2_1_1) { OcpiRelaxations = OcpiValidatorRelaxations.UrlAsAnyString })
            .AddOcpiValidation()
            .BuildServiceProvider()
            .GetRequiredService<IOcpiValidator<OcpiBusinessDetails>>();
        
        validator.Validate(new OcpiBusinessDetails { Name = "something", Website = "blah blah blah" }).IsValid.Should().BeTrue();
        validator.Validate(new OcpiBusinessDetails { Name = "something", Website = "www.example.com" }).IsValid.Should().BeTrue();
        validator.Validate(new OcpiBusinessDetails { Name = "something", Website = "http://www.example.com" }).IsValid.Should().BeTrue();
        validator.Validate(new OcpiBusinessDetails { Name = "something", Website = "https://www.example.com:8080/path" }).IsValid.Should().BeTrue();
    }
}
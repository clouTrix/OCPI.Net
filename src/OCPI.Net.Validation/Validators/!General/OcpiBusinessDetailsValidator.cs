using FluentValidation;
using OCPI.Validation;

namespace OCPI.Contracts;

internal class OcpiBusinessDetailsValidator : OcpiValidator<OcpiBusinessDetails>
{
    public OcpiBusinessDetailsValidator(
        IOcpiValidator<OcpiImage> imageValidator)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Website)
            .ValidUrl().Unless(_ => OcpiRelaxations.Has(OcpiValidatorRelaxations.UrlAsAnyString));
        
        RuleFor(x => x.Logo!)
            .SetValidator(imageValidator);
    }
}

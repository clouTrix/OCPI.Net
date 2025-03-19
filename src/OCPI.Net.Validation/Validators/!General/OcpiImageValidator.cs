using FluentValidation;
using OCPI.Validation;

namespace OCPI.Contracts;

internal class OcpiImageValidator : OcpiValidator<OcpiImage>
{
    public OcpiImageValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .ValidUrl().Unless(_ => OcpiRelaxations.Has(OcpiValidatorRelaxations.UrlAsAnyString));

        RuleFor(x => x.Thumbnail)
            .ValidUrl().Unless(_ => OcpiRelaxations.Has(OcpiValidatorRelaxations.UrlAsAnyString));;

        RuleFor(x => x.Category)
            .NotEmpty()
            .IsInEnum();

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(4);

        RuleFor(x => x.Width)
            .MaxSymbols(5);

        RuleFor(x => x.Height)
            .MaxSymbols(5);
    }
}

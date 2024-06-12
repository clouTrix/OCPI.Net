using FluentValidation;
using OCPI.Contracts;

namespace OCPI.Validation.Validators.Session;

internal partial class OcpiSessionValidator: OcpiValidator<OcpiSession> {
    public OcpiSessionValidator(
        IOcpiValidator<OcpiLocation> locationValidator
    ) {
        Unless(ActionType.Patch, () =>
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();

            WhenOcpiVersionAbove("2.2", () =>
            {
                RuleFor(x => x.CountryCode).NotEmpty();
                RuleFor(x => x.PartyId).NotEmpty();
            });

            WhenOcpiVersionBelow("2.2", () =>
            {
                RuleFor(x => x.StartDateTime).NotEmpty();
                // RuleFor(x => x.Location).NotNull();
            });
        });
        
        
        WhenOcpiVersionAbove("2.2", () => {
            RuleFor(x => x.CountryCode)
                .IsInEnum();

            RuleFor(x => x.PartyId)
                .MaximumLength(3);
        });
        
        RuleFor(x => x.Id)
            .MaximumLength(36);

        RuleFor(x => x.LastUpdated)
            .NotEmpty()
            .ValidDateTime();

        //TODO: add all required validations
        
        WhenOcpiVersionBelow("2.2", () => {
            RuleFor(x => x.StartDateTime)
                .ValidDateTime();

            RuleFor(x => x.Location)
                .SetValidator(locationValidator!);
        });
    }
}
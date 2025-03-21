﻿using FluentValidation;
using OCPI.Contracts;

namespace OCPI.Validation;

internal class OcpiCredentialsValidator : OcpiValidator<OcpiCredentials>
{
    public OcpiCredentialsValidator(
        IOcpiValidator<OcpiCredentialsRole> credentialsRoleValidator)
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .NoWhitespace()
            .InUnicodeRange(0x0021, 0x007E)
            .MaximumLength(64);

        RuleFor(x => x.Url)
            .NotEmpty()
            .ValidUrl().Unless(_ => OcpiRelaxations.Has(OcpiValidatorRelaxations.UrlAsAnyString))
            .MaximumLength(2048);

        WhenOcpiVersionBelow("2.2", () => {
            RuleFor(x => x.PartyId).NotEmpty();
            RuleFor(x => x.CountryCode).NotEmpty();
        });
        
        WhenOcpiVersionAbove("2.2", () => {
            RuleFor(x => x.Roles)
                .NotEmpty();

            RuleForEach(x => x.Roles!)
                .SetValidator(credentialsRoleValidator);
        });
    }
}

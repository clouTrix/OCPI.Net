﻿using FluentValidation;
using OCPI.Validation;

namespace OCPI.Contracts;

internal class OcpiEvseValidator : OcpiValidator<OcpiEvse>
{
    public OcpiEvseValidator(string httpMethod) : base(httpMethod)
    {
        When(x => !IsPatch, () =>
        {
            JsonRuleFor(x => x.Uid).NotEmpty();
            JsonRuleFor(x => x.Status).NotEmpty();

            JsonRuleFor(x => x.Connectors)
            .NotEmpty();
        });

        JsonRuleFor(x => x.Uid)
            .MaximumLength(36);

        JsonRuleFor(x => x.EvseId)
            .MaximumLength(48);

        JsonRuleFor(x => x.Status)
            .MustValidEnum();

        RuleForEach(x => x.StatusSchedule)
            .SetValidator(new OcpiStatusScheduleValidator(httpMethod));

        RuleForEach(x => x.Capabilities)
            .MustValidEnum();

        JsonRuleFor(x => x.FloorLevel)
            .MaximumLength(4);

        JsonRuleFor(x => x.Coordinates!)
            .SetValidator(new OcpiGeolocationValidator(httpMethod));

        JsonRuleFor(x => x.PhysicalReference)
            .MaximumLength(16);

        RuleForEach(x => x.Directions)
            .SetValidator(new OcpiDisplayTextValidator(httpMethod));

        RuleForEach(x => x.ParkingRestrictions)
            .MustValidEnum();

        RuleForEach(x => x.Images)
            .SetValidator(new OcpiImageValidator(httpMethod));

        JsonRuleFor(x => x.LastUpdated)
            .NotEmpty()
            .MustValidDateTime();
    }
}
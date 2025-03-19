using FluentValidation;
using Microsoft.AspNetCore.Http;
using OCPI.Contracts;

namespace OCPI.Validation;

public class OcpiValidationContext(ActionType actionType, OcpiVersion? ocpiVersion)
{
    public ActionType ActionType { get; set; } = actionType;
    public OcpiVersion? OcpiVersion { get; set; } = ocpiVersion;

    public OcpiValidatorRelaxations OcpiRelaxations { get; set; } = OcpiValidatorRelaxations.None;

    public OcpiValidationContext(HttpRequest request)
        : this(request.Method.ToActionType(), request.GetCurrentOcpiVersion())
    { }
}

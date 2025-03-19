namespace OCPI.Contracts;

[Flags]
public enum OcpiValidatorRelaxations
{
    None           = 0b_0000_0000,
    UrlAsAnyString = 0b_0000_0001
}

public static class OcpiValidatorRelaxationExtensions
{
    public static bool Has(this OcpiValidatorRelaxations self, OcpiValidatorRelaxations flag)
        => (self & flag) == flag;
}
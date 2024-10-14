using BitzArt.EnumToMemberValue;

namespace OCPI;

/// <summary>
/// Marks the OCPI protocol elements that were introduced in a specific OCPI version.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate, Inherited = false)]
public class OcpiIntroducedAttribute : Attribute
{
    public OcpiVersion Version { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OcpiIntroducedAttribute"/> by string representation of the OCPI version that introduced this value.
    /// </summary>
    /// <param name="introduced">A string representation of the OCPI Version that introduced this class/object/method</param>
    public OcpiIntroducedAttribute(string introduced)
    {
        Version = introduced.ToEnum<OcpiVersion>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OcpiIntroducedAttribute"/> by the OCPI version that introduced this value.
    /// </summary>
    /// <param name="introduced">The OCPI Version that introduced this class/object/method</param>
    public OcpiIntroducedAttribute(OcpiVersion introduced)
    {
        Version = introduced;
    }
}

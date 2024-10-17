using BitzArt.EnumToMemberValue;

namespace OCPI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class OcpiVersionAttribute : Attribute
{
    public IEnumerable<OcpiVersion> Versions { get; set; }

    public OcpiVersionAttribute(string versions)
    {
        Versions = versions.Split(',').Select(x => x.Trim().ToEnum<OcpiVersion>());
    }
}

namespace wfc.referential.Application.Constants;

public static class CacheKeys
{
    private const string InstancePrefix = "ReferentialCache:";

    // 🔹 Entité : City
    public static class City
    {
        public const string Prefix = InstancePrefix + "City_";
    }

    // 🔹 Entité : Region
    public static class Region
    {
        public const string Prefix = InstancePrefix + "Region_";
    }

    // 🔹 Entité : Agency
    public static class Agency
    {
        public const string Prefix = InstancePrefix + "Agency_";
    }

    // 🔹 Entité : TypeDefinition
    public static class TypeDefinition
    {
        public const string Prefix = InstancePrefix + "TypeDefinition_";
    }

    // 🔹 Entité : ParamTypes
    public static class ParamType
    {
        public const string Prefix = InstancePrefix + "ParamType_";
    }
}

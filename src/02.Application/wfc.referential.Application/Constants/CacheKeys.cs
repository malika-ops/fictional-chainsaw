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

    // 🔹 Entité : IdentityDocuments
    public static class IdentityDocument
    {
        public const string Prefix = InstancePrefix + "IdentityDocument_";
    }

    // 🔹 Entité : CountryIdentityDocument
    public static class CountryIdentityDocument
    {
        public const string Prefix = InstancePrefix + "CountryIdentityDocument_";
    }

    // 🔹 Entité : Corridor
    public static class Corridor
    {
        public const string Prefix = InstancePrefix + "Corridor_";
    }

    // 🔹 Entité : Tax
    public static class Tax
    {
        public const string Prefix = InstancePrefix + "Tax_";
    }

    // 🔹 Entité : TaxRuleDetail
    public static class TaxRuleDetail
    {
        public const string Prefix = InstancePrefix + "TaxRuleDetail_";
    }

    // 🔹 Entité : Product
    public static class ProductCache
    {
        public const string Prefix = InstancePrefix + "Product_";
    }
}

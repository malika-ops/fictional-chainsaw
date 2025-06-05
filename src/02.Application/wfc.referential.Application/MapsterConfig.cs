using wfc.referential.Application.Agencies.Mappings;
using wfc.referential.Application.AgencyTiers.Mappings;
using wfc.referential.Application.Banks.Mappings;
using wfc.referential.Application.Cities.Mappings;
using wfc.referential.Application.Corridors.Mappings;
using wfc.referential.Application.Countries.Mappings;
using wfc.referential.Application.CountryIdentityDocs.Mappings;
using wfc.referential.Application.Currencies.Mappings;
using wfc.referential.Application.IdentityDocuments.Mappings;
using wfc.referential.Application.MonetaryZones.Mappings;
using wfc.referential.Application.ParamTypes.Mappings;
using wfc.referential.Application.PartnerAccounts.Mappings;
using wfc.referential.Application.PartnerCountries.Mappings;
using wfc.referential.Application.Partners.Mappings;
using wfc.referential.Application.Pricings.Mappings;
using wfc.referential.Application.Products.Mappings;
using wfc.referential.Application.Regions.Mappings;
using wfc.referential.Application.Sectors.Mappings;
using wfc.referential.Application.Services.Mappings;
using wfc.referential.Application.SupportAccounts.Mappings;
using wfc.referential.Application.Taxes.Mappings;
using wfc.referential.Application.TaxRuleDetails.Mappings;
using wfc.referential.Application.Tiers.Mappings;
using wfc.referential.Application.TypeDefinitions.Mappings;

namespace wfc.referential.Application;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        RegionMappings.Register();
        CityMappings.Register();
        CountryMappings.Register();
        CurrencyMappings.Register();
        MonetaryZoneMappings.Register();
        ParamTypeMappings.Register();
        TypeDefinitionMappings.Register();
        SectorMappings.Register();
        AgencyMappings.Register();
        TaxMappings.Register();
        BankMappings.Register();
        PartnerAccountMappings.Register();
        ProductMappings.Register();
        ServiceMappings.Register();
        IdentityDocumentMappings.Register();
        CorridorMappings.Register();
        CountryIdentityDocMappings.Register();
        TaxRuleDetailMappings.Register();
        TierMappings.Register();
        AgencyTierMappings.Register();
        SupportAccountMappings.Register();
        PartnerMappings.Register();
        PartnerCountryMappings.Register();
        PricingMappings.Register();
    }
}
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using wfc.referential.Application.Data;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.TierAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.TaxRuleDetailAggregate;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.PartnerCountryAggregate;

namespace wfc.referential.Infrastructure.Data;
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<MonetaryZone> MonetaryZones => Set<MonetaryZone>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<TypeDefinition> TypeDefinitions => Set<TypeDefinition>();
    public DbSet<ParamType> ParamTypes => Set<ParamType>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<CountryIdentityDoc> CountryIdentityDocs => Set<CountryIdentityDoc>();
    public DbSet<PartnerAccount> PartnerAccounts => Set<PartnerAccount>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Tier> Tiers => Set<Tier>();
    public DbSet<AgencyTier> AgencyTiers => Set<AgencyTier>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<IdentityDocument> IdentityDocuments => Set<IdentityDocument>();
    public DbSet<Corridor> Corridors => Set<Corridor>();
    public DbSet<TaxRuleDetail> TaxRuleDetails => Set<TaxRuleDetail>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<SupportAccount> SupportAccounts => Set<SupportAccount>();
    public DbSet<PartnerCountry> PartnerCountries => Set<PartnerCountry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
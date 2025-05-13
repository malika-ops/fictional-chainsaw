using Microsoft.EntityFrameworkCore;
using wfc.referential.Domain.BankAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.TaxAggregate;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;

namespace wfc.referential.Application.Data;

public interface IApplicationDbContext
{
    DbSet<MonetaryZone> MonetaryZones { get; }
    DbSet<Country> Countries { get; }
    DbSet<Region> Regions { get; }
    DbSet<City> Cities { get; }
    DbSet<Sector> Sectors { get; }
    DbSet<TypeDefinition> TypeDefinitions { get; }
    DbSet<ParamType> ParamTypes { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<Tax> Taxes { get; }
    DbSet<Product> Products { get; }
    DbSet<Service> Services { get; }
    DbSet<Bank> Banks { get; }
    DbSet<PartnerAccount> PartnerAccounts { get; }
    DbSet<Corridor> Corridors { get; }
    DbSet<CountryIdentityDoc> CountryIdentityDocs { get; }
    public DbSet<Partner> Partners { get; }
    public DbSet<SupportAccount> SupportAccounts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
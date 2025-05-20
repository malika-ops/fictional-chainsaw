using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Behaviors.Interceptors;
using BuildingBlocks.Infrastructure.CachingManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using wfc.referential.Application.Data;
using wfc.referential.Application.Interfaces;
using wfc.referential.Infrastructure.Data;
using wfc.referential.Infrastructure.Data.Repositories;
using wfc.referential.Infrastructure.Persistence.Repositories;

namespace wfc.referential.Infrastructure;
public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructureServices
        (this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        // Add services to the container.
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        var redisConnection = configuration.GetConnectionString("Redis");
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "ReferentialCache:";
        });

        builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        builder.Services.AddScoped<CacheService>();
        builder.Services.AddScoped<ICacheService, CacheService>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(
                sp.GetRequiredService<IConfiguration>().GetConnectionString("Redis"),
                ignoreUnknown: true
            );
            configuration.AbortOnConnectFail = false; // Important
            return ConnectionMultiplexer.Connect(configuration);
        });

        // 🔹 Initialiser la base de données ici
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var cacheservice = scope.ServiceProvider.GetRequiredService<CacheService>();
                try
                {
                    DbInitializer.SeedWithCache(context, cacheservice);
                    Console.WriteLine("Base de données initialisée avec succès !");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'initialisation de la base : {ex.Message}");
                }
            }
        }

        builder.Services.AddScoped<IMonetaryZoneRepository, MonetaryZoneRepository>();
        builder.Services.AddScoped<ICountryRepository, CountryRepository>();
        builder.Services.AddScoped<IMonetaryZoneRepository, MonetaryZoneRepository>();
        builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        builder.Services.AddScoped<IRegionRepository, RegionRepository>();
        builder.Services.AddScoped<ICityRepository, CityRepository>();
        builder.Services.AddScoped<ITypeDefinitionRepository, TypeDefinitionRepository>();
        builder.Services.AddScoped<IParamTypeRepository, ParamTypeRepository>();
        builder.Services.AddScoped<ISectorRepository, SectorRepository>();
        builder.Services.AddScoped<IAgencyRepository, AgencyRepository>();
        builder.Services.AddScoped<ITaxRepository, TaxRepository>();
        builder.Services.AddScoped<IBankRepository, BankRepository>();
        builder.Services.AddScoped<IPartnerAccountRepository, PartnerAccountRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
        builder.Services.AddScoped<IIdentityDocumentRepository, IdentityDocumentRepository>();
        builder.Services.AddScoped<ICorridorRepository, CorridorRepository>();
        builder.Services.AddScoped<ITaxRuleDetailRepository, TaxRuleDetailRepository>();

        builder.Services.AddScoped<ICountryIdentityDocRepository, CountryIdentityDocRepository>();
        builder.Services.AddScoped<ITierRepository, TierRepository>();
        builder.Services.AddScoped<IAgencyTierRepository, AgencyTierRepository>();
        builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
        builder.Services.AddScoped<ISupportAccountRepository, SupportAccountRepository>();
        builder.Services.AddScoped<IPartnerCountryRepository, PartnerCountryRepository>();

        return builder;
    }
}

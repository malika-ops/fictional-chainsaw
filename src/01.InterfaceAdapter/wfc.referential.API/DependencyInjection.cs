using BuildingBlocks.Core.Behaviors.Handlers;
using BuildingBlocks.Core.Encryption;
using wfc.referential.API.Endpoints;
using wfc.referential.API.Features;
using static BuildingBlocks.Core.Behaviors.Handlers.GlobalExceptionHandler;

namespace wfc.referential.API;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddApiServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {

        builder.Services.AddSingleton<Encryption>();

        var redisConnection = configuration.GetConnectionString("Redis");
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "ReferentialCache:";
        });

        //Exceptions Handler
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "en", "fr", "es" };
            options.SetDefaultCulture("en")
                   .AddSupportedCultures(supportedCultures)
                   .AddSupportedUICultures(supportedCultures);
        });
        builder.Services.AddSingleton<IExceptionMessageProvider, ResourceExceptionMessageProvider>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


        return builder;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.UseExceptionHandler(opt => { });

        return app;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapAgencyEndpoints();
        app.MapCityEndpoints();
        app.MapCountryEndpoints();
        app.MapRegionEndpoints();
        app.MapAffiliateEndpoints();
        app.MapBankEndpoints();
        app.MapContractEndpoints();
        app.MapContractDetailsEndpoints();
        app.MapCountryIdentityDocEndpoints();
        app.MapIdentityDocumentEndpoints();
        app.MapServiceEndpoints();
        app.MapParamTypeEndpoints();
        app.MapPartnerAccountEndpoints();
        app.MapPartnerCountryEndpoints();
        app.MapPartnerEndpoints();
        app.MapCountryServiceEndpoints();
        app.MapSectorEndpoints();
        app.MapTaxEndpoints();
        app.MapTaxRuleDetailEndpoints();
        app.MapTierEndpoints();
        app.MapAgencyTierEndpoints();
        app.MapSupportAccountEndpoints();
        app.MapCorridorEndpoints();
        app.MapCurrencyEndpoints();
        app.MapMonetaryZoneEndpoints();
        app.MapPricingEndpoints();
        app.MapProductEndpoints();
        app.MapTypeDefinitionEndpoints();
        app.MapControleEndpoints();
        app.MapServiceControleEndpoints();
        app.MapOperatorEndpoints();

        return app;
    }
}
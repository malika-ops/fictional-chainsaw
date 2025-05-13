using BuildingBlocks.Core.Encryption;
using BuildingBlocks.Core.Behaviors.Handlers;
using FastEndpoints;
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
        app.UseFastEndpoints();
 

        return app;
    }
}

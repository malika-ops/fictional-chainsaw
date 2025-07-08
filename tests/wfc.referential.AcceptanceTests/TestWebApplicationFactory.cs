using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Audit.Interface;
using BuildingBlocks.Core.Kafka.Producer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using wfc.referential.Application.Interfaces;
using Xunit;

namespace wfc.referential.AcceptanceTests;
// Collection fixture pour partager la factory entre tous les tests
[CollectionDefinition("WebApp Collection")]
public class WebAppCollection : ICollectionFixture<TestWebApplicationFactory>
{
}

public class TestWebApplicationFactory : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Dictionary<Type, Mock> _mocks = new();

    public TestWebApplicationFactory()
    {
        _factory = CreateFactory();
    }

    public WebApplicationFactory<Program> Factory => _factory;

    // Accès centralisé aux mocks
    public Mock<T> GetMock<T>() where T : class
    {
        var type = typeof(T);
        if (!_mocks.ContainsKey(type))
        {
            _mocks[type] = new Mock<T>();
        }
        return (Mock<T>)_mocks[type];
    }

    // Réinitialisation des mocks entre les tests
    public void ResetMocks()
    {
        foreach (var mock in _mocks.Values)
        {
            mock.Reset();
        }
    }

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                // Optimisations de performance
                builder.ConfigureServices(services =>
                {
                    // Désactiver les logs non essentiels
                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        options.MinLevel = LogLevel.Warning;
                    });

                    // Enregistrement centralisé des mocks
                    RegisterMocks(services);
                });
            });
    }

    private void RegisterMocks(IServiceCollection services)
    {
        // Création et enregistrement des mocks
        services.AddSingleton(GetMock<IAgencyTierRepository>().Object);
        services.AddSingleton(GetMock<IAgencyRepository>().Object);
        services.AddSingleton(GetMock<ITierRepository>().Object);
        services.AddSingleton(GetMock<IAffiliateRepository>().Object);
        services.AddSingleton(GetMock<IParamTypeRepository>().Object);
        services.AddSingleton(GetMock<ITypeDefinitionRepository>().Object);
        services.AddSingleton(GetMock<ICountryRepository>().Object);
        services.AddSingleton(GetMock<IPricingRepository>().Object);
        services.AddSingleton(GetMock<ICorridorRepository>().Object);
        services.AddSingleton(GetMock<ICityRepository>().Object);
        services.AddSingleton(GetMock<IRegionRepository>().Object);
        services.AddSingleton(GetMock<ISectorRepository>().Object);
        services.AddSingleton(GetMock<IPartnerRepository>().Object);
        services.AddSingleton(GetMock<ISupportAccountRepository>().Object);
        services.AddSingleton(GetMock<ICacheService>().Object);
        services.AddSingleton(GetMock<IBankRepository>().Object);
        services.AddSingleton(GetMock<IContractDetailsRepository>().Object);
        services.AddSingleton(GetMock<IControleRepository>().Object);
        services.AddSingleton(GetMock<IContractRepository>().Object);
        services.AddSingleton(GetMock<ICountryIdentityDocRepository>().Object);
        services.AddSingleton(GetMock<IIdentityDocumentRepository>().Object);
        services.AddSingleton(GetMock<ICountryServiceRepository>().Object);
        services.AddSingleton(GetMock<ICurrencyRepository>().Object);
        services.AddSingleton(GetMock<IMonetaryZoneRepository>().Object);
        services.AddSingleton(GetMock<IProducerService>().Object);
        services.AddSingleton(GetMock<IPartnerCountryRepository>().Object);
        services.AddSingleton(GetMock<ICurrentUserContext>().Object);
        services.AddSingleton(GetMock<IPartnerAccountRepository>().Object);
        services.AddSingleton(GetMock<IServiceRepository>().Object);
        services.AddSingleton(GetMock<IProductRepository>().Object);
        services.AddSingleton(GetMock<IServiceControleRepository>().Object);
        services.AddSingleton(GetMock<ITaxRuleDetailRepository>().Object);
        services.AddSingleton(GetMock<ITaxRepository>().Object);
    }

    public void Dispose()
    {
        _factory?.Dispose();
    }
}
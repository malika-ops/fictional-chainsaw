using AutoFixture;
using BuildingBlocks.Application.Interfaces;
using BuildingBlocks.Core.Audit.Interface;
using BuildingBlocks.Core.Kafka.Producer;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AffiliateAggregate;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.CorridorAggregate;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.OperatorAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.ProductAggregate;
using wfc.referential.Domain.ServiceAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests;
//Base acceptance tests
[Collection("WebApp Collection")]
public abstract class BaseAcceptanceTests : IDisposable
{
    protected readonly HttpClient _client;
    protected readonly TestWebApplicationFactory _factory;
    protected readonly IFixture _fixture;
    // Propriétés pour accéder aux mocks
    protected Mock<IOperatorRepository> _operatorRepoMock => _factory.GetMock<IOperatorRepository>();
    protected Mock<IAgencyTierRepository> _agencyTierRepoMock => _factory.GetMock<IAgencyTierRepository>();
    protected Mock<IAgencyRepository> _agencyRepoMock => _factory.GetMock<IAgencyRepository>();
    protected Mock<ITierRepository> _tierRepoMock => _factory.GetMock<ITierRepository>();
    protected Mock<IAffiliateRepository> _affiliateRepoMock => _factory.GetMock<IAffiliateRepository>();
    protected Mock<IParamTypeRepository> _paramTypeRepoMock => _factory.GetMock<IParamTypeRepository>();
    protected Mock<ITypeDefinitionRepository> _typeDefinitionRepoMock => _factory.GetMock<ITypeDefinitionRepository>();
    protected Mock<ICountryRepository> _countryRepoMock => _factory.GetMock<ICountryRepository>();
    protected Mock<IPricingRepository> _pricingRepoMock => _factory.GetMock<IPricingRepository>();
    protected Mock<ICorridorRepository> _corridorRepoMock => _factory.GetMock<ICorridorRepository>();
    protected Mock<ICityRepository> _cityRepoMock => _factory.GetMock<ICityRepository>();
    protected Mock<IRegionRepository> _regionRepoMock => _factory.GetMock<IRegionRepository>();
    protected Mock<ISectorRepository> _sectorRepoMock => _factory.GetMock<ISectorRepository>();
    protected Mock<IPartnerRepository> _partnerRepoMock => _factory.GetMock<IPartnerRepository>();
    protected Mock<ISupportAccountRepository> _supportAccountRepoMock => _factory.GetMock<ISupportAccountRepository>();
    protected Mock<ICacheService> _cacheMock => _factory.GetMock<ICacheService>();
    protected Mock<IBankRepository> _bankRepoMock => _factory.GetMock<IBankRepository>();
    protected Mock<IContractDetailsRepository> _contractDetailsRepoMock => _factory.GetMock<IContractDetailsRepository>();
    protected Mock<IControleRepository> _controleRepoMock => _factory.GetMock<IControleRepository>();
    protected Mock<IContractRepository> _contractRepoMock => _factory.GetMock<IContractRepository>();
    protected Mock<ICountryIdentityDocRepository> _countryIdentityDocRepoMock => _factory.GetMock<ICountryIdentityDocRepository>();
    protected Mock<IIdentityDocumentRepository> _identityDocumentRepoMock => _factory.GetMock<IIdentityDocumentRepository>();
    protected Mock<ICountryServiceRepository> _countryServiceRepoMock => _factory.GetMock<ICountryServiceRepository>();
    protected Mock<ICurrencyRepository> _currencyRepoMock => _factory.GetMock<ICurrencyRepository>();
    protected Mock<IMonetaryZoneRepository> _monetaryZoneRepoMock => _factory.GetMock<IMonetaryZoneRepository>();
    protected Mock<IProducerService> _producerServiceMock => _factory.GetMock<IProducerService>();
    protected Mock<IPartnerCountryRepository> _partnerCountryRepoMock => _factory.GetMock<IPartnerCountryRepository>();
    protected Mock<ICurrentUserContext> _currentUserServiceMock => _factory.GetMock<ICurrentUserContext>();
    protected Mock<IPartnerAccountRepository> _partnerAccountRepoMock => _factory.GetMock<IPartnerAccountRepository>();
    protected Mock<IServiceRepository> _serviceRepoMock => _factory.GetMock<IServiceRepository>();
    protected Mock<IProductRepository> _productRepoMock => _factory.GetMock<IProductRepository>();
    protected Mock<IServiceControleRepository> _serviceControlRepoMock => _factory.GetMock<IServiceControleRepository>();
    protected Mock<ITaxRuleDetailRepository> _taxRuleDetailsRepoMock => _factory.GetMock<ITaxRuleDetailRepository>();
    protected Mock<ITaxRepository> _taxRepoMock => _factory.GetMock<ITaxRepository>();

    protected BaseAcceptanceTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.Factory.CreateClient();
        _fixture = new Fixture();
        // Réinitialiser les mocks avant chaque test
        _factory.ResetMocks();

        // Configuration des mocks par défaut
        SetupDefaultFixtureBehaviors();
    }

    private void SetupDefaultFixtureBehaviors()
    {
        _fixture.Customize<Agency>(composer =>
           composer.FromFactory(() =>
               Agency.Create(
                   AgencyId.Of(_fixture.Create<Guid>()),
                   _fixture.Create<string>().Substring(0, 6), // Code de 6 caractères
                   _fixture.Create<string>(),
                   "TST",
                   _fixture.Create<string>(),
                   null,
                   "123-456-7890",
                   "123-456-7891",
                   "Test Sheet",
                   "ACC123",
                   "12345",
                   null, null, null, null, null, null, null,
                   CityId.Of(_fixture.Create<Guid>()),
                   null, null, null, null, null, null
               )));
        // Configuration pour Partner
        _fixture.Customize<Partner>(composer =>
            composer.FromFactory(() =>
                Partner.Create(
                    PartnerId.Of(_fixture.Create<Guid>()),
                    _fixture.Create<string>().Substring(0, 6), // Code de 6 caractères
                    _fixture.Create<string>(),
                    "TST",
                    _fixture.Create<string>(),
                    null,
                    "123-456-7890",
                    "123-456-7891",
                    "Test Sheet",
                    "ACC123",
                    _fixture.Create<string>(),
                    "12345",
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>(),
                    _fixture.Create<string>()
                )));

        _fixture.Customize<Country>(composer =>
          composer.FromFactory(() =>
              Country.Create(
                  CountryId.Of(_fixture.Create<Guid>()),
                  _fixture.Create<string>().Substring(0, 6), // Code de 6 caractères
                  _fixture.Create<string>(),
                  "TST",
                  _fixture.Create<string>(),
                  _fixture.Create<string>(),
                  _fixture.Create<string>(),
                  _fixture.Create<string>(),
                  _fixture.Create<bool>(),
                  _fixture.Create<bool>(),
                  _fixture.Create<int>(),
                  MonetaryZoneId.Of(_fixture.Create<Guid>()),
                  CurrencyId.Of(_fixture.Create<Guid>())
              )));
        // Configuration pour Tier
        _fixture.Customize<Tier>(composer =>
            composer.FromFactory(() =>
                Tier.Create(
                    TierId.Of(_fixture.Create<Guid>()),
                    _fixture.Create<string>(),
                    _fixture.Create<string>()
                )));

        _fixture.Customize<Currency>(composer =>
      composer.FromFactory(() =>
          Currency.Create(
              CurrencyId.Of(_fixture.Create<Guid>()),
              _fixture.Create<string>().Substring(0, 6), // Code de 6 caractères
              _fixture.Create<string>(),
              "TST",
              _fixture.Create<string>(),
              _fixture.Create<int>()
          )));

        _fixture.Customize<MonetaryZone>(composer =>
       composer.FromFactory(() =>
           MonetaryZone.Create(
               MonetaryZoneId.Of(_fixture.Create<Guid>()),
               _fixture.Create<string>().Substring(0, 6), // Code de 6 caractères
               _fixture.Create<string>(),
               _fixture.Create<string>()
           )));

        _fixture.Customize<Service>(composer =>
     composer.FromFactory(() =>
         Service.Create(
             ServiceId.Of(_fixture.Create<Guid>()),
             _fixture.Create<string>().Substring(0, 6), // Code de 6 caractères
             _fixture.Create<string>(),
             _fixture.Create<FlowDirection>(),
             _fixture.Create<bool>(),
             ProductId.Of(_fixture.Create<Guid>())
         )));


        _fixture.Customize<Corridor>(composer =>
      composer.FromFactory(() =>
          Corridor.Create(
              CorridorId.Of(_fixture.Create<Guid>()),
              CountryId.Of(_fixture.Create<Guid>()),
              CountryId.Of(_fixture.Create<Guid>()),
              CityId.Of(_fixture.Create<Guid>()),
              CityId.Of(_fixture.Create<Guid>()),
              AgencyId.Of(_fixture.Create<Guid>()),
              AgencyId.Of(_fixture.Create<Guid>())
          )));

        _fixture.Customize<Operator>(composer =>
     composer.FromFactory(() =>
        Operator.Create(
            OperatorId.Of(_fixture.Create<Guid>()),
            _fixture.Create<string>().Substring(0, 6), 
            _fixture.Create<string>().Substring(0, 8), 
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            _fixture.Create<string>() + "@email.com",
            "+212600000000",
            _fixture.Create<OperatorType>(),
            _fixture.Create<Guid>(),
            _fixture.Create<Guid>()
        )));

        _fixture.Customize<Affiliate>(composer =>
      composer.FromFactory(() =>
          Affiliate.Create(
              AffiliateId.Of(_fixture.Create<Guid>()),
              _fixture.Create<string>(),
              _fixture.Create<string>(),
              _fixture.Create<string>(),
              DateTime.Now,
              _fixture.Create<string>(),
              _fixture.Create<string>(),
              _fixture.Create<decimal>(),
              _fixture.Create<string>(),
              _fixture.Create<string>(),
              _fixture.Create<string>(),
              CountryId.Of(_fixture.Create<Guid>())
          )));
    }

    public virtual void Dispose()
    {
    }
}
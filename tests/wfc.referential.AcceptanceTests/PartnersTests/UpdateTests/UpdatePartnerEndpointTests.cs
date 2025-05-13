using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.UpdateTests;

public class UpdatePartnerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();
    private readonly Mock<ISectorRepository> _sectorRepoMock = new();
    private readonly Mock<ICityRepository> _cityRepoMock = new();

    public UpdatePartnerEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ISectorRepository>();
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // Set up sector and city mocks to return valid entities
                var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
                var regionId = Guid.NewGuid();

                
                var city = City.Create(CityId.Of(cityId), "C001", "Test City", "timezone", "taxzone", new RegionId(regionId), null);

        
                _sectorRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<SectorId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Sector.Create(SectorId.Of(sectorId), "S001", "Test Sector", city));

                // Corriger le mock pour utiliser la valeur Guid au lieu de l'objet CityId
                _cityRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(city);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_sectorRepoMock.Object);
                services.AddSingleton(_cityRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to create a test partner
    private static Partner CreateTestPartner(Guid id, string code, string label, NetworkMode networkMode)
    {
        var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var regionId = Guid.NewGuid();

        
        var city = City.Create(CityId.Of(cityId), "C001", "Test City", "timezone", "taxzone", new RegionId(regionId), null);


        var sector = Sector.Create(SectorId.Of(sectorId), "S001", "Test Sector", city);

        return Partner.Create(
            new PartnerId(id),
            code,
            label,
            networkMode,
            PaymentMode.PrePaye,
            "ID" + code,
            Domain.SupportAccountAggregate.SupportAccountType.Commun,
            "IDNUM" + code,
            "Standard",
            "AUX" + code,
            "ICE" + code,
            "/logos/logo.png",
            sector,
            city
        );
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var oldPartner = CreateTestPartner(id, "PTN001", "Old Partner", NetworkMode.Franchise);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);

        _repoMock.Setup(r => r.GetByCodeAsync("PTN002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);   // Code is unique

        _repoMock.Setup(r => r.GetByIdentificationNumberAsync("IDNUM002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);   // Identification number is unique

        _repoMock.Setup(r => r.GetByICEAsync("ICE002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);   // ICE is unique

        Partner? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                        It.IsAny<CancellationToken>()))
                 .Callback<Partner, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            Code = "PTN002",
            Label = "New Partner Name",
            NetworkMode = "Succursale",
            PaymentMode = "PostPaye",
            IdPartner = "ID002",
            SupportAccountType = "Individuel",
            IdentificationNumber = "IDNUM002",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE002",
            Logo = "/logos/new-logo.png",
            SectorId = sectorId,
            CityId = cityId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("PTN002");
        updated.Label.Should().Be("New Partner Name");
        updated.NetworkMode.Should().Be(NetworkMode.Succursale);
        updated.PaymentMode.Should().Be(PaymentMode.PostPaye);
        updated.IdPartner.Should().Be("ID002");
        updated.IdentificationNumber.Should().Be("IDNUM002");
        updated.TaxRegime.Should().Be("Simplified");
        updated.AuxiliaryAccount.Should().Be("AUX002");
        updated.ICE.Should().Be("ICE002");
        updated.Logo.Should().Be("/logos/new-logo.png");
        updated.SupportAccountType.Should().Be(Domain.SupportAccountAggregate.SupportAccountType.Individuel);
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} allows changing the enabled status")]
    public async Task Put_ShouldAllowChangingEnabledStatus_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var oldPartner = CreateTestPartner(id, "PTN001", "Test Partner", NetworkMode.Franchise);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);

        _repoMock.Setup(r => r.GetByCodeAsync("PTN001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);   // Same code is ok for same partner

        _repoMock.Setup(r => r.GetByIdentificationNumberAsync("IDNUMPTN001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);   // Same identification number is ok for same partner

        _repoMock.Setup(r => r.GetByICEAsync("ICEPTN001", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(oldPartner);   // Same ICE is ok for same partner

        Partner? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                        It.IsAny<CancellationToken>()))
                 .Callback<Partner, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            Code = "PTN001",
            Label = "Test Partner",
            NetworkMode = "Franchise",
            PaymentMode = "PrePaye",
            IdPartner = "IDPTN001",
            SupportAccountType = "Commun",
            IdentificationNumber = "IDNUMPTN001",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUXPTN001",
            ICE = "ICEPTN001",
            Logo = "/logos/logo.png",
            SectorId = sectorId,
            CityId = cityId,
            IsEnabled = false // Changed from true to false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 400 when Code is missing")]
    public async Task Put_ShouldReturn400_WhenCodeMissing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var payload = new
        {
            PartnerId = id,
            // Code intentionally omitted
            Label = "New Partner Name",
            NetworkMode = "Succursale",
            PaymentMode = "PostPaye",
            IdPartner = "ID002",
            SupportAccountType = "Individuel",
            IdentificationNumber = "IDNUM002",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE002",
            Logo = "/logos/new-logo.png",
            SectorId = sectorId,
            CityId = cityId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PUT /api/partners/{id} returns 400 when Partner doesn't exist")]
    public async Task Put_ShouldReturn404_WhenPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);

        var payload = new
        {
            PartnerId = id,
            Code = "PTN002",
            Label = "New Partner",
            NetworkMode = "Succursale",
            PaymentMode = "PostPaye",
            IdPartner = "ID002",
            SupportAccountType = "Individuel",
            IdentificationNumber = "IDNUM002",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE002",
            Logo = "/logos/new-logo.png",
            SectorId = sectorId,
            CityId = cityId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/partners/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Partner with ID {id} not found");

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
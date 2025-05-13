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

namespace wfc.referential.AcceptanceTests.PartnersTests.CreateTests;

public class CreatePartnerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();
    private readonly Mock<ISectorRepository> _sectorRepoMock = new();
    private readonly Mock<ICityRepository> _cityRepoMock = new();

    public CreatePartnerEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ISectorRepository>();
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Partner p, CancellationToken _) => p);

                // Set up sector and city mocks to return valid entities
                var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

                // Create the city first
                var city = City.Create(CityId.Of(cityId), "C001", "Test City", "timezone", "taxzone", new RegionId(Guid.NewGuid()), null);

                _cityRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(city);

                // Now create the sector WITH the city
                _sectorRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<SectorId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Sector.Create(SectorId.Of(sectorId), "S001", "Test Sector", city));

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_sectorRepoMock.Object);
                services.AddSingleton(_cityRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/partners returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        Partner capturedCreatePartner = null;
        _repoMock
            .Setup(r => r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()))
            .Callback<Partner, CancellationToken>((p, _) => capturedCreatePartner = p)
            .ReturnsAsync((Partner p, CancellationToken _) => p);

        _repoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner)null);

        _repoMock
            .Setup(r => r.GetByIdentificationNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner)null);

        _repoMock
            .Setup(r => r.GetByICEAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner)null);

        var payload = new
        {
            Code = "PTN001",
            Label = "Partner 1",
            NetworkMode = "Franchise",
            PaymentMode = "PrePaye",
            IdPartner = "ID001",
            SupportAccountType = "Commun",
            IdentificationNumber = "ID12345",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE12345",
            Logo = "/logos/logo.png",
            SectorId = sectorId,
            CityId = cityId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Vérifier que la méthode a été appelée
        _repoMock.Verify(r => r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()), Times.Once);

        // Assertions basées sur ce que nous savons être présent
        capturedCreatePartner.Should().NotBeNull();
        capturedCreatePartner.Code.Should().Be(payload.Code);
        capturedCreatePartner.Label.Should().Be(payload.Label);
        capturedCreatePartner.NetworkMode.ToString().Should().Be(payload.NetworkMode);
        capturedCreatePartner.PaymentMode.ToString().Should().Be(payload.PaymentMode);
        capturedCreatePartner.IdPartner.Should().Be(payload.IdPartner);
        capturedCreatePartner.IdentificationNumber.Should().Be(payload.IdentificationNumber);
        capturedCreatePartner.ICE.Should().Be(payload.ICE);
        capturedCreatePartner.SupportAccountType.ToString().Should().Be(payload.SupportAccountType);
        capturedCreatePartner.Sector.Id.Value.Should().Be(sectorId);
        capturedCreatePartner.City.Id.Value.Should().Be(cityId);
    }

    [Fact(DisplayName = "POST /api/partners returns 400 & problem-details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var invalidPayload = new
        {
            // Code intentionally omitted to trigger validation error
            Label = "Partner 1",
            NetworkMode = "Franchise",
            PaymentMode = "PrePaye",
            IdPartner = "ID001",
            SupportAccountType = "Commun",
            IdentificationNumber = "ID12345",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE12345",
            Logo = "/logos/logo.png",
            SectorId = sectorId,
            CityId = cityId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", invalidPayload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Bad Request");
        root.GetProperty("status").GetInt32().Should().Be(400);

        root.GetProperty("errors")
            .GetProperty("code")[0].GetString()
            .Should().Be("Code is required");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "PTN001";
        var sectorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Tell the repo mock that the code already exists
        // Create the city first
        var existingCity = City.Create(CityId.Of(cityId), "C001", "Test City", "timezone", "taxzone", new RegionId(Guid.NewGuid()), null);

        // Now create the sector WITH the city
        var existingSector = Sector.Create(SectorId.Of(sectorId), "S001", "Test Sector", existingCity);

        var existingPartner = Partner.Create(
            PartnerId.Of(Guid.NewGuid()),
            duplicateCode,
            "Existing Partner",
            NetworkMode.Franchise,
            PaymentMode.PrePaye,
            "IDEXIST",
            Domain.SupportAccountAggregate.SupportAccountType.Commun,
            "IDNUM123",
            "Standard",
            "AUXEXIST",
            "ICEEXIST",
            "/logos/existing.png",
            existingSector,
            existingCity
        );

        _repoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPartner);

        var payload = new
        {
            Code = duplicateCode,
            Label = "New Partner",
            NetworkMode = "Succursale",
            PaymentMode = "PostPaye",
            IdPartner = "ID002",
            SupportAccountType = "Individuel",
            IdentificationNumber = "ID67890",
            TaxRegime = "Simplified",
            AuxiliaryAccount = "AUX002",
            ICE = "ICE67890",
            Logo = "/logos/new.png",
            SectorId = sectorId,
            CityId = cityId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Partner with code {duplicateCode} already exists.");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }

    [Fact(DisplayName = "POST /api/partners returns 400 when Sector is not found")]
    public async Task Post_ShouldReturn400_WhenSectorNotFound()
    {
        // Arrange
        var nonExistentSectorId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Setup sector repository to return null for this ID
        _sectorRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<SectorId>(id => id.Value == nonExistentSectorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sector?)null);

        var payload = new
        {
            Code = "PTN001",
            Label = "Partner 1",
            NetworkMode = "Franchise",
            PaymentMode = "PrePaye",
            IdPartner = "ID001",
            SupportAccountType = "Commun",
            IdentificationNumber = "ID12345",
            TaxRegime = "Standard",
            AuxiliaryAccount = "AUX001",
            ICE = "ICE12345",
            Logo = "/logos/logo.png",
            SectorId = nonExistentSectorId,
            CityId = cityId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/partners", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Sector with ID {nonExistentSectorId} not found");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddPartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
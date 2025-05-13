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
using wfc.referential.Domain.PartnerAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.SectorAggregate;
using Xunit;
using wfc.referential.Domain.RegionAggregate;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.CreateTests;

public class CreateSupportAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repoMock = new();
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();

    public CreateSupportAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ICacheService>();

                // Set up mock behavior (echoes entity back, as if EF saved it)
                _repoMock
                    .Setup(r => r.AddSupportAccountAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((SupportAccount p, CancellationToken _) => p);

                // Set up partner mock to return valid entity
                var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var sectorId = Guid.NewGuid();
                var cityId = Guid.NewGuid();

                // Create city first
                var city = City.Create(CityId.Of(cityId), "C001", "Test City", "timezone", "taxzone", new RegionId(Guid.NewGuid()), null);

                // Create sector with city
                var sector = Sector.Create(SectorId.Of(sectorId), "S001", "Test Sector", city);

                var partner = Partner.Create(
                    PartnerId.Of(partnerId),
                    "P001",
                    "Test Partner",
                    NetworkMode.Franchise,
                    PaymentMode.PrePaye,
                    "ID001",
                    SupportAccountType.Commun,
                    "IDNUM001",
                    "Standard",
                    "AUX001",
                    "ICE001",
                    "/logos/logo.png",
                    sector,
                    city
                );

                _partnerRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(partner);

                // Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_partnerRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customizedFactory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/support-accounts returns 200 and Guid when request is valid")]
    public async Task Post_ShouldReturn200_AndId_WhenRequestIsValid()
    {
        // Arrange
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var payload = new
        {
            Code = "SA001",
            Name = "Support Account 1",
            Threshold = 10000.00m,
            Limit = 20000.00m,
            AccountBalance = 5000.00m,
            AccountingNumber = "ACC001",
            PartnerId = partnerId,
            SupportAccountType = "Commun"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert (FluentAssertions)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        // Verify repository interaction
        _repoMock.Verify(r =>
            r.AddSupportAccountAsync(It.Is<SupportAccount>(s =>
                    s.Code == payload.Code &&
                    s.Name == payload.Name &&
                    s.Threshold == payload.Threshold &&
                    s.Limit == payload.Limit &&  // Verify Limit value
                    s.AccountBalance == payload.AccountBalance &&
                    s.AccountingNumber == payload.AccountingNumber &&
                    s.Partner.Id.Value == partnerId &&
                    s.SupportAccountType == SupportAccountType.Commun),
                    It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact(DisplayName = "POST /api/support-accounts returns 400 & problem-details when Code is missing")]
    public async Task Post_ShouldReturn400_WhenCodeIsMissing()
    {
        // Arrange
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var invalidPayload = new
        {
            // Code intentionally omitted to trigger validation error
            Name = "Support Account 1",
            Threshold = 10000.00m,
            Limit = 20000.00m,
            AccountBalance = 5000.00m,
            AccountingNumber = "ACC001",
            PartnerId = partnerId,
            SupportAccountType = "Commun"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", invalidPayload);
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
            r.AddSupportAccountAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "when validation fails, the command handler should not be executed");
    }

    [Fact(DisplayName = "POST /api/support-accounts returns 400 when Limit is negative")]
    public async Task Post_ShouldReturn400_WhenLimitIsNegative()
    {
        // Arrange
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var payload = new
        {
            Code = "SA001",
            Name = "Support Account 1",
            Threshold = 10000.00m,
            Limit = -5000.00m,  // Negative limit
            AccountBalance = 5000.00m,
            AccountingNumber = "ACC001",
            PartnerId = partnerId,
            SupportAccountType = "Commun"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("errors")
            .GetProperty("limit")[0].GetString()
            .Should().Be("Limit must be non-negative");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddSupportAccountAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/support-accounts returns 400 when Code already exists")]
    public async Task Post_ShouldReturn400_WhenCodeAlreadyExists()
    {
        // Arrange 
        const string duplicateCode = "SA001";
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Tell the repo mock that the code already exists
        var partner = _partnerRepoMock.Object.GetByIdAsync(new PartnerId(partnerId), CancellationToken.None).Result;

        var existingAccount = SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            duplicateCode,
            "Existing Support Account",
            10000.00m,
            15000.00m,
            5000.00m,
            "ACC001",
            partner,
            SupportAccountType.Commun
        );

        _repoMock
            .Setup(r => r.GetByCodeAsync(duplicateCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        var payload = new
        {
            Code = duplicateCode,
            Name = "New Support Account",
            Threshold = 15000.00m,
            Limit = 25000.00m,
            AccountBalance = 7500.00m,
            AccountingNumber = "ACC002",
            PartnerId = partnerId,
            SupportAccountType = "Individuel"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Support account with code {duplicateCode} already exists.");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddSupportAccountAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "no insertion should happen when the code is already taken");
    }

    [Fact(DisplayName = "POST /api/support-accounts returns 400 when Partner is not found")]
    public async Task Post_ShouldReturn400_WhenPartnerNotFound()
    {
        // Arrange
        var nonExistentPartnerId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Setup partner repository to return null for this ID
        _partnerRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<PartnerId>(id => id.Value == nonExistentPartnerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        var payload = new
        {
            Code = "SA001",
            Name = "Support Account 1",
            Threshold = 10000.00m,
            Limit = 20000.00m,
            AccountBalance = 5000.00m,
            AccountingNumber = "ACC001",
            PartnerId = nonExistentPartnerId,
            SupportAccountType = "Commun"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        var error = root.GetProperty("errors").GetString();

        error.Should().Be($"Partner with ID {nonExistentPartnerId} not found");

        // Handler must NOT attempt to add the entity
        _repoMock.Verify(r =>
            r.AddSupportAccountAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "POST /api/support-accounts returns 400 when Threshold is negative")]
    public async Task Post_ShouldReturn400_WhenThresholdIsNegative()
    {
        // Arrange
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var payload = new
        {
            Code = "SA001",
            Name = "Support Account 1",
            Threshold = -1000.00m,  // Negative threshold
            Limit = 20000.00m,
            AccountBalance = 5000.00m,
            AccountingNumber = "ACC001",
            PartnerId = partnerId,
            SupportAccountType = "Commun"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/support-accounts", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var root = doc!.RootElement;
        root.GetProperty("errors")
            .GetProperty("threshold")[0].GetString()
            .Should().Be("Threshold must be non-negative");

        // The handler must NOT be reached
        _repoMock.Verify(r =>
            r.AddSupportAccountAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
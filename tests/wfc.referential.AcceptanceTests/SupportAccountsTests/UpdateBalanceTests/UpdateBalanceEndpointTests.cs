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

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.UpdateBalanceTests;

public class UpdateBalanceEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repoMock = new();

    public UpdateBalanceEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                           It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to create a test support account
    private static SupportAccount CreateTestSupportAccount(Guid id, string code, string name, decimal threshold, decimal limit, decimal balance)
    {
        var partnerId = Guid.NewGuid();


        var cityId = Guid.NewGuid();
        var city = City.Create(CityId.Of(cityId), "C001", "Test City", "timezone", "taxzone", new RegionId(Guid.NewGuid()), null);


        var sectorId = Guid.NewGuid();
        var sector = Sector.Create(SectorId.Of(sectorId), "S001", "Test Sector", city);

        var partner = Partner.Create(
            PartnerId.Of(partnerId),
            "P" + code,
            "Partner for " + name,
            NetworkMode.Franchise,
            PaymentMode.PrePaye,
            "ID" + code,
            SupportAccountType.Commun,
            "IDNUM" + code,
            "Standard",
            "AUX" + code,
            "ICE" + code,
            "/logos/logo.png",
            sector,
            city
        );

        return SupportAccount.Create(
            new SupportAccountId(id),
            code,
            name,
            threshold,
            limit,
            balance,
            "ACC" + code,
            partner,
            SupportAccountType.Commun
        );
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance returns 200 and updates only the balance")]
    public async Task Patch_ShouldReturn200_AndUpdateOnlyBalance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Support Account 1", 10000.00m, 20000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        SupportAccount? updated = null;
        _repoMock.Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                        It.IsAny<CancellationToken>()))
                 .Callback<SupportAccount, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            SupportAccountId = id,
            NewBalance = 7500.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}/balance", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.AccountBalance.Should().Be(7500.00m);  // Balance should change
        updated.Code.Should().Be("SA001"); // Other fields should not change
        updated.Name.Should().Be("Support Account 1");
        updated.Threshold.Should().Be(10000.00m);
        updated.Limit.Should().Be(20000.00m); // Limit should not change

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                          It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance returns 400 when account doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount?)null);

        var payload = new
        {
            SupportAccountId = id,
            NewBalance = 7500.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}/balance", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Support account with ID {id} not found");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance returns 400 when balance is negative")]
    public async Task Patch_ShouldReturn400_WhenBalanceIsNegative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Support Account 1", 10000.00m, 20000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        var payload = new
        {
            SupportAccountId = id,
            NewBalance = -1000.00m // Negative balance
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}/balance", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("newBalance")[0].GetString()
            .Should().Be("Balance cannot be negative");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id}/balance doesn't affect the Limit value")]
    public async Task Patch_ShouldNotAffectLimit_WhenUpdatingBalance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalLimit = 20000.00m;
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Support Account 1", 10000.00m, originalLimit, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        SupportAccount? updated = null;
        _repoMock.Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                        It.IsAny<CancellationToken>()))
                 .Callback<SupportAccount, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            SupportAccountId = id,
            NewBalance = 10000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}/balance", JsonContent.Create(payload));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.Limit.Should().Be(originalLimit); // Limit should remain unchanged
        updated.AccountBalance.Should().Be(10000.00m);  // Balance should change

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                          It.IsAny<CancellationToken>()),
                          Times.Once);
    }
}
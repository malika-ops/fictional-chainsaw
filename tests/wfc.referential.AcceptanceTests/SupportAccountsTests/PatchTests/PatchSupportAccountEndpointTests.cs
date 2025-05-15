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
using Xunit;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.PatchTests;

public class PatchSupportAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repoMock = new();
    private readonly Mock<IPartnerRepository> _partnerRepoMock = new();

    public PatchSupportAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                // Set up partner mock to return valid entity
                var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var sectorId = Guid.NewGuid();
                var cityId = Guid.NewGuid();

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
                    null, // IdParent
                    null, // CommissionAccountId
                    null, // ActivityAccountId
                    null  // SupportAccountId
                );

                _partnerRepoMock
                    .Setup(r => r.GetByIdAsync(It.IsAny<PartnerId>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(partner);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_partnerRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to create a test support account
    private static SupportAccount CreateTestSupportAccount(Guid id, string code, string name, decimal threshold, decimal limit, decimal balance)
    {
        var partnerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var sectorId = Guid.NewGuid();
        var cityId = Guid.NewGuid();

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
            null, // IdParent
            null, // CommissionAccountId
            null, // ActivityAccountId
            null  // SupportAccountId
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

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Old Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        _repoMock.Setup(r => r.GetByCodeAsync("SA002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount?)null);   // Code is unique

        SupportAccount? updated = null;
        _repoMock.Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                        It.IsAny<CancellationToken>()))
                 .Callback<SupportAccount, CancellationToken>((s, _) => updated = s)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002",
            // Other fields intentionally omitted - should not change
            Threshold = 12000.00m, // Including Threshold
            Limit = 20000.00m // Including Limit
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("SA002");     // Should change
        updated.Threshold.Should().Be(12000.00m);  // Should change
        updated.Limit.Should().Be(20000.00m);   // Should change
        updated.Name.Should().Be("Old Support Account"); // Should not change
        updated.AccountBalance.Should().Be(5000.00m); // Should not change
        updated.SupportAccountType.Should().Be(SupportAccountType.Commun); // Should not change
        updated.IsEnabled.Should().BeTrue(); // Should remain enabled

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} allows updating only the Limit field")]
    public async Task Patch_ShouldAllowUpdatingOnlyLimit()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Test Support Account", 10000.00m, 15000.00m, 5000.00m);

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
            Limit = 25000.00m // Only updating the Limit
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Limit.Should().Be(25000.00m); // Should change
        updated.Code.Should().Be("SA001"); // Should not change
        updated.Name.Should().Be("Test Support Account"); // Should not change
        updated.Threshold.Should().Be(10000.00m); // Should not change

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} allows changing only the enabled status")]
    public async Task Patch_ShouldAllowChangingOnlyEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Test Support Account", 10000.00m, 15000.00m, 5000.00m);

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
            IsEnabled = false // Change from enabled to disabled
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse(); // Should be disabled
        updated.Code.Should().Be("SA001"); // Should not change
        updated.Name.Should().Be("Test Support Account"); // Should not change
        updated.Limit.Should().Be(15000.00m); // Should not change

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} returns 400 when Limit is negative")]
    public async Task Patch_ShouldReturn400_WhenLimitIsNegative()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(id, "SA001", "Test Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(supportAccount);

        var payload = new
        {
            SupportAccountId = id,
            Limit = -5000.00m // Negative limit
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

       
        responseContent.Should().NotBeEmpty();

        // On peut aussi vérifier que le mot "Limit" apparaît dans la réponse,
        // ce qui serait une indication que l'erreur concerne le champ Limit
        responseContent.ToLowerInvariant().Should().Contain("limit");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} returns 400 when account doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenAccountDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((SupportAccount?)null);

        var payload = new
        {
            SupportAccountId = id,
            Threshold = 15000.00m,
            Limit = 25000.00m
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Support account not found");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/support-accounts/{id} returns 400 when new code already exists")]
    public async Task Patch_ShouldReturn400_WhenNewCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var existing = CreateTestSupportAccount(existingId, "SA002", "Existing Support Account", 12000.00m, 18000.00m, 6000.00m);
        var target = CreateTestSupportAccount(id, "SA001", "Target Support Account", 10000.00m, 15000.00m, 5000.00m);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("SA002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // Duplicate code

        var payload = new
        {
            SupportAccountId = id,
            Code = "SA002"  // This code already exists for another account
        };

        // Act
        var response = await _client.PatchAsync($"/api/support-accounts/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Support account with code SA002 already exists.");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
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
using wfc.referential.Domain.PartnerAccountAggregate;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.PartnersTests.PatchTests;

public class PatchPartnerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();
    private readonly Mock<IPartnerAccountRepository> _partnerAccountRepoMock = new();
    private readonly Mock<ISupportAccountRepository> _supportAccountRepoMock = new();

    public PatchPartnerEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<IPartnerAccountRepository>();
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

                // Default noop for Update
                _repoMock
                    .Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_partnerAccountRepoMock.Object);
                services.AddSingleton(_supportAccountRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to create a test partner
    private static Partner CreateTestPartner(Guid id, string code, string label, NetworkMode networkMode)
    {
        return Partner.Create(
            new PartnerId(id),
            code,
            label,
            networkMode,
            PaymentMode.PrePaye,
            "Test Type",
            Domain.SupportAccountAggregate.SupportAccountType.Commun,
            "IDNUM" + code,
            "Standard",
            "AUX" + code,
            "ICE" + code,
            "10.5",
            "/logos/logo.png",
            null, // IdParent
            null, // CommissionAccountId
            null, // ActivityAccountId
            null  // SupportAccountId
        );
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Old Partner", NetworkMode.Franchise);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        _repoMock.Setup(r => r.GetByCodeAsync("PTN002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);   // Code is unique

        Partner? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Partner, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            Code = "PTN002",
            // Other fields intentionally omitted - should not change
            Label = "New Partner Name"
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.Code.Should().Be("PTN002");  // Should change
        updated.Label.Should().Be("New Partner Name");  // Should change
        updated.NetworkMode.Should().Be(NetworkMode.Franchise); // Should not change
        updated.PaymentMode.Should().Be(PaymentMode.PrePaye); // Should not change
        updated.IsEnabled.Should().BeTrue(); // Should remain enabled

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                          Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} allows changing only the enabled status")]
    public async Task Patch_ShouldAllowChangingOnlyEnabledStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(id, "PTN001", "Test Partner", NetworkMode.Franchise);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(partner);

        Partner? updated = null;
        _repoMock.Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                It.IsAny<CancellationToken>()))
                 .Callback<Partner, CancellationToken>((p, _) => updated = p)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            PartnerId = id,
            IsEnabled = false // Change from enabled to disabled
            // Other fields intentionally omitted
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse(); // Should be disabled
        updated.Code.Should().Be("PTN001"); // Should not change
        updated.Label.Should().Be("Test Partner"); // Should not change

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                  It.IsAny<CancellationToken>()),
                           Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} returns 400 when partner doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenPartnerDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Partner?)null);

        var payload = new
        {
            PartnerId = id,
            Label = "New Partner Name"
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Partner not found");

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/partners/{id} returns 400 when new code already exists")]
    public async Task Patch_ShouldReturn400_WhenNewCodeAlreadyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingId = Guid.NewGuid();

        var existing = CreateTestPartner(existingId, "PTN002", "Existing Partner", NetworkMode.Succursale);
        var target = CreateTestPartner(id, "PTN001", "Target Partner", NetworkMode.Franchise);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("PTN002", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing); // Duplicate code

        var payload = new
        {
            PartnerId = id,
            Code = "PTN002"  // This code already exists for another partner
        };

        // Act
        var response = await _client.PatchAsync($"/api/partners/{id}", JsonContent.Create(payload));
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Partner with code PTN002 already exists.");

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
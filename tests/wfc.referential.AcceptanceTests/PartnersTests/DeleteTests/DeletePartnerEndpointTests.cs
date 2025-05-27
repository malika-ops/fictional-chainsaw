using System.Linq.Expressions;
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

namespace wfc.referential.AcceptanceTests.PartnersTests.DeleteTests;

public class DeletePartnerEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IPartnerRepository> _repoMock = new();
    private readonly Mock<ISupportAccountRepository> _supportAccountRepoMock = new();

    public DeletePartnerEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IPartnerRepository>();
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                   It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_supportAccountRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // Helper to build dummy partners quickly
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

    [Fact(DisplayName = "DELETE /api/partners/{id} returns 200 when partner exists")]
    public async Task Delete_ShouldReturn200_WhenPartnerExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(
            id,
            "PTN001",
            "Test Partner",
            NetworkMode.Franchise
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        _supportAccountRepoMock
            .Setup(r => r.GetByConditionAsync(It.IsAny<Expression<Func<SupportAccount, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportAccount>()); // No support accounts associated

        // Capture the entity passed to Update
        Partner? updatedPartner = null;
        _repoMock
            .Setup(r => r.UpdatePartnerAsync(It.IsAny<Partner>(), It.IsAny<CancellationToken>()))
            .Callback<Partner, CancellationToken>((p, _) => updatedPartner = p)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updatedPartner!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                                                 Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} returns 400 when partner is not found")]
    public async Task Delete_ShouldReturn400_WhenPartnerNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Partner?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Partner not found");

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                                                 Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/partners/{id} returns 400 when partner has support accounts")]
    public async Task Delete_ShouldReturn400_WhenPartnerHasSupportAccounts()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partner = CreateTestPartner(
            id,
            "PTN001",
            "Test Partner",
            NetworkMode.Franchise
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<PartnerId>(pid => pid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(partner);

        // Mock that partner has support accounts
        var supportAccount = SupportAccount.Create(
            SupportAccountId.Of(Guid.NewGuid()),
            "SA001",
            "Support Account",
            10000.00m,
            20000.00m,
            5000.00m,
            "ACC001"
        );

        _supportAccountRepoMock
            .Setup(r => r.GetByConditionAsync(
                It.IsAny<Expression<Func<SupportAccount, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SupportAccount> { supportAccount });

        // Act
        var response = await _client.DeleteAsync($"/api/partners/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Cannot delete partner with existing support accounts");

        _repoMock.Verify(r => r.UpdatePartnerAsync(It.IsAny<Partner>(),
                                                 It.IsAny<CancellationToken>()),
                                                 Times.Never);
    }
}
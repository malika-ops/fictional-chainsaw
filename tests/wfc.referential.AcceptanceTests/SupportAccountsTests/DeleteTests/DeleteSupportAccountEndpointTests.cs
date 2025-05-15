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

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.DeleteTests;

public class DeleteSupportAccountEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repoMock = new();

    public DeleteSupportAccountEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISupportAccountRepository>();
                services.RemoveAll<ICacheService>();

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

    // Helper to build dummy support accounts quickly
    private static SupportAccount CreateTestSupportAccount(Guid id, string code, string name, decimal threshold, decimal limit, decimal balance)
    {
        var partnerId = Guid.NewGuid();
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

    [Fact(DisplayName = "DELETE /api/support-accounts/{id} returns 200 when account exists")]
    public async Task Delete_ShouldReturn200_WhenAccountExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supportAccount = CreateTestSupportAccount(
            id,
            "SA001",
            "Test Support Account",
            10000.00m,
            20000.00m,
            5000.00m
        );

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(supportAccount);

        // Capture the entity passed to Update
        SupportAccount? updatedAccount = null;
        _repoMock
            .Setup(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(), It.IsAny<CancellationToken>()))
            .Callback<SupportAccount, CancellationToken>((s, _) => updatedAccount = s)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/support-accounts/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updatedAccount!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                                                         Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/support-accounts/{id} returns 400 when account is not found")]
    public async Task Delete_ShouldReturn400_WhenAccountNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(It.Is<SupportAccountId>(sid => sid.Value == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportAccount?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/support-accounts/{id}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Support account not found");

        _repoMock.Verify(r => r.UpdateSupportAccountAsync(It.IsAny<SupportAccount>(),
                                                         It.IsAny<CancellationToken>()),
                                                         Times.Never);
    }
}
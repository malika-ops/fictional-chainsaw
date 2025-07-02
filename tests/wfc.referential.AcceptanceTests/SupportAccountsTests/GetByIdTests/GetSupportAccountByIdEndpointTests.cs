using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.SupportAccountAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SupportAccountsTests.GetByIdTests;

public class GetSupportAccountByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISupportAccountRepository> _repo = new();

    public GetSupportAccountByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<ISupportAccountRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static SupportAccount Make(Guid id, string code = "SUPPORT-ACCOUNT-001", string? name = null, bool enabled = true)
    {
        var supportAccount = SupportAccount.Create(
            id: SupportAccountId.Of(id),
            code: code,
            description: name ?? $"SupportAccount-{code}",
            threshold: 1000.00m,
            limit: 5000.00m,
            accountBalance: 1000.00m,
            accountingNumber: $"ACC-{code}"
        );

        if (!enabled)
            supportAccount.Disable();

        return supportAccount;
    }

    private record SupportAccountDto(Guid Id, string Code, string Description, bool IsEnabled);

    [Fact(DisplayName = "GET /api/support-accounts/{id} → 404 when SupportAccount not found")]
    public async Task Get_ShouldReturn404_WhenSupportAccountNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((SupportAccount?)null);

        var res = await _client.GetAsync($"/api/support-accounts/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/support-accounts/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/support-accounts/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<SupportAccountId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/support-accounts/{id} → 200 for disabled SupportAccount")]
    public async Task Get_ShouldReturn200_WhenSupportAccountDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "SUPPORT-ACCOUNT-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(SupportAccountId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/support-accounts/{id}");
        var dto = await res.Content.ReadFromJsonAsync<SupportAccountDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.AgencyAggregate;
using wfc.referential.Domain.AgencyTierAggregate;
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.AgencyTierTests.PatchTests;

public class PatchAgencyTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IAgencyTierRepository> _repoMock = new();

    public PatchAgencyTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAgencyTierRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.UpdateAsync(
                                   It.IsAny<AgencyTier>(),
                                   It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /* ---------- helpers ---------- */

    private static AgencyTier At(Guid id, Guid agencyId, Guid tierId, string code, bool enabled = true)
    {
        return AgencyTier.Create(
            AgencyTierId.Of(id),
            new AgencyId(agencyId),
            new TierId(tierId),
            code,
            password: string.Empty,
            isEnabled: enabled);
    }

    private async Task<HttpResponseMessage> PatchJsonAsync(string url, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await _client.SendAsync(req);
    }

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }

    /* ---------- tests ---------- */

    // 1) Happy-path ----------------------------------------------------------
    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} returns 200 when partial update succeeds")]
    public async Task Patch_ShouldReturn200_WhenPatchSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var original = At(id, agencyId, tierId, "OLD", enabled: true);

        _repoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(original);

        AgencyTier? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<AgencyTier>(), It.IsAny<CancellationToken>()))
                 .Callback<AgencyTier, CancellationToken>((at, _) => saved = at)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            AgencyTierId = id,
            Code = "NEW",
            IsEnabled = false
        };

        // Act
        var resp = await PatchJsonAsync($"/api/agencyTiers/{id}", payload);
        var result = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        saved!.Code.Should().Be("NEW");
        saved.IsEnabled.Should().BeFalse();
        // unchanged fields untouched
        saved.AgencyId.Should().Be(original.AgencyId);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AgencyTier>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Duplicate code ------------------------------------------------------
    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} returns 400 when new Code already exists")]
    public async Task Patch_ShouldReturn400_WhenCodeDuplicate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var tierId = Guid.NewGuid();

        var target = At(id, agencyId, tierId, "UNI");
        var duplicate = At(Guid.NewGuid(), agencyId, tierId, "DUP");

        _repoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByCodeAsync("DUP", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(duplicate);

        var payload = new { AgencyTierId = id, Code = "DUP" };

        // Act
        var resp = await PatchJsonAsync($"/api/agencyTiers/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("AgencyTier with code 'DUP' already exists for this agency & tier.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AgencyTier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 3) Not-found -----------------------------------------------------------
    [Fact(DisplayName = "PATCH /api/agencyTiers/{id} returns 404 when AgencyTier not found")]
    public async Task Patch_ShouldReturn404_WhenEntityMissing()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(AgencyTierId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((AgencyTier?)null);

        var payload = new { AgencyTierId = id, Code = "ANY" };

        var resp = await PatchJsonAsync($"/api/agencyTiers/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AgencyTier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
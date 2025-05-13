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
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.PatchTests;

public class PatchTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITierRepository> _repoMock = new();

    public PatchTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITierRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);   // common stub

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /* ---------- helpers ---------- */

    private static Tier Tr(string name, string desc, Guid id, bool enabled = true)
        => Tier.Create(new TierId(id), name, desc, enabled);

    private static async Task<HttpResponseMessage> PatchJsonAsync(HttpClient client, string url, object body)
    {
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var req = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
        return await client.SendAsync(req);
    }

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }

    /* ---------- tests ---------- */

    // 1) Happy-path – partial update
    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 200 when partial update succeeds")]
    public async Task Patch_ShouldReturn200_WhenPatchSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var original = Tr("Silver", "Silver tier", id);

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TierId>(t => t.Value == id),
                                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(original);

        Tier? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()))
                 .Callback<Tier, CancellationToken>((t, _) => saved = t)
                 .Returns(Task.CompletedTask);

        var payload = new
        {
            TierId = id,
            Name = "Silver-Plus",
            IsEnabled = false
        };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", payload);
        var result = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        saved!.Name.Should().Be("Silver-Plus");
        saved.IsEnabled.Should().BeFalse();               // updated
        saved.Description.Should().Be("Silver tier");     // unchanged

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Duplicate Name
    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 400 when new Name already exists")]
    public async Task Patch_ShouldReturn400_WhenNameDuplicate()
    {
        // Arrange
        var idTarget = Guid.NewGuid();
        var target = Tr("Standard", "desc", idTarget);

        var dupTier = Tr("Gold", "dup", Guid.NewGuid());

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TierId>(t => t.Value == idTarget),
                                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByNameAsync("Gold", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(dupTier);

        var payload = new { TierId = idTarget, Name = "Gold" };

        // Act
        var resp = await PatchJsonAsync(_client, $"/api/tiers/{idTarget}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Tier 'Gold' already exists.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 3) Validation – empty GUID
    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 400 when TierId is empty GUID")]
    public async Task Patch_ShouldReturn400_WhenIdEmpty()
    {
        var payload = new { TierId = Guid.Empty, Name = "Anything" };

        var resp = await PatchJsonAsync(_client,
                   "/api/tiers/00000000-0000-0000-0000-000000000000",
                   payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "TierId")
            .Should().Be("TierId cannot be empty.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 4) Tier not found
    [Fact(DisplayName = "PATCH /api/tiers/{id} returns 400 when tier not found")]
    public async Task Patch_ShouldReturn400_WhenTierMissing()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(It.Is<TierId>(t => t.Value == id),
                                            It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null); // not found

        var payload = new { TierId = id, Description = "Will fail" };

        var resp = await PatchJsonAsync(_client, $"/api/tiers/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Tier not found.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}
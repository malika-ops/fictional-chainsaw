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
using wfc.referential.Domain.TierAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TierTests.UpdateTests;

public class UpdateTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITierRepository> _repoMock = new();

    public UpdateTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<ITierRepository>();
                s.RemoveAll<ICacheService>();

                // generic stub – per-test callbacks override where needed
                _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /* ---------- helpers ---------- */

    private static Tier Tr(string name, string desc, Guid id, bool enabled = true)
        => Tier.Create(new TierId(id), name, desc, enabled);

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }

    /* ---------- tests ---------- */

    // 1) Happy-path ----------------------------------------------------------
    [Fact(DisplayName = "PUT /api/tiers/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var old = Tr("Silver", "Old desc", id);

        _repoMock.Setup(r => r.GetByIdAsync(new TierId(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(old);

        _repoMock.Setup(r => r.GetByNameAsync("Silver-Plus", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);      // name is unique

        Tier? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()))
                 .Callback<Tier, CancellationToken>((t, _) => saved = t);

        var payload = new
        {
            TierId = id,
            Name = "Silver-Plus",
            Description = "Updated description",
            IsEnabled = false
        };

        // Act
        var resp = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);
        var result = await resp.Content.ReadFromJsonAsync<Guid>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(id);

        saved!.Name.Should().Be("Silver-Plus");
        saved.Description.Should().Be("Updated description");
        saved.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    // 2) Validation – Name missing ------------------------------------------
    [Fact(DisplayName = "PUT /api/tiers/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        var id = Guid.NewGuid();

        var payload = new
        {
            TierId = id,
            // Name omitted
            Description = "Desc",
            IsEnabled = true
        };

        var resp = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "name")
            .Should().Be("Name is required.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 3) Duplicate name ------------------------------------------------------
    [Fact(DisplayName = "PUT /api/tiers/{id} returns 400 when new Name already exists")]
    public async Task Put_ShouldReturn400_WhenNameDuplicate()
    {
        var idTarget = Guid.NewGuid();
        var existing = Tr("Gold", "dup", Guid.NewGuid());
        var target = Tr("Silver", "old", idTarget);

        _repoMock.Setup(r => r.GetByIdAsync(new TierId(idTarget), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(target);

        _repoMock.Setup(r => r.GetByNameAsync("Gold", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var payload = new
        {
            TierId = idTarget,
            Name = "Gold",           // duplicate
            Description = "Updated",
            IsEnabled = true
        };

        var resp = await _client.PutAsJsonAsync($"/api/tiers/{idTarget}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Tier 'Gold' already exists.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 4) Empty GUID ----------------------------------------------------------
    [Fact(DisplayName = "PUT /api/tiers/{id} returns 400 when TierId is empty GUID")]
    public async Task Put_ShouldReturn400_WhenIdEmpty()
    {
        var payload = new
        {
            TierId = Guid.Empty,
            Name = "Anything",
            Description = "Desc",
            IsEnabled = true
        };

        var resp = await _client.PutAsJsonAsync(
                        "/api/tiers/00000000-0000-0000-0000-000000000000",
                        payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "TierId")
            .Should().Be("TierId cannot be empty.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    // 5) Tier not found ------------------------------------------------------
    [Fact(DisplayName = "PUT /api/tiers/{id} returns 404 when tier not found")]
    public async Task Put_ShouldReturn400_WhenTierMissing()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(new TierId(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);

        var payload = new
        {
            TierId = id,
            Name = "Missing",
            Description = "N/A",
            IsEnabled = true
        };

        var resp = await _client.PutAsJsonAsync($"/api/tiers/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Tier not found.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}

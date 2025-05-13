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

namespace wfc.referential.AcceptanceTests.TierTests.DeleteTests;

public class DeleteTierEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ITierRepository> _repoMock = new();

    public DeleteTierEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customised = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<ITierRepository>();
                s.RemoveAll<ICacheService>();

                _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Tier>(),
                                                   It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);

                s.AddSingleton(_repoMock.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = customised.CreateClient();
    }

    /* --------------------- helpers --------------------- */

    private static Tier MakeTier(Guid id, string name = "Standard")
        => Tier.Create(new TierId(id), name, "descr", true);

    private static string FirstError(JsonElement errs, string key)
    {
        foreach (var p in errs.EnumerateObject())
            if (p.NameEquals(key) || p.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                return p.Value[0].GetString()!;
        throw new KeyNotFoundException($"error key '{key}' not found");
    }

    /* ---------------------- tests ---------------------- */

    [Fact(DisplayName = "DELETE /api/tiers/{id} returns 200 when deletion succeeds")]
    public async Task Delete_ShouldReturn200_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tier = MakeTier(id);

        _repoMock.Setup(r => r.GetByIdAsync(new TierId(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(tier);

        Tier? saved = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()))
                 .Callback<Tier, CancellationToken>((t, _) => saved = t);

        // Act
        var resp = await _client.DeleteAsync($"/api/tiers/{id}");
        var success = await resp.Content.ReadFromJsonAsync<bool>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        success.Should().BeTrue();

        saved!.IsEnabled.Should().BeFalse();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/tiers/{id} returns 400 when id is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenIdIsEmpty()
    {
        // Act
        var resp = await _client.DeleteAsync("/api/tiers/00000000-0000-0000-0000-000000000000");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        FirstError(doc!.RootElement.GetProperty("errors"), "TierId")
            .Should().Be("TierId must be a non-empty GUID.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }

    [Fact(DisplayName = "DELETE /api/tiers/{id} returns 400 when tier not found")]
    public async Task Delete_ShouldReturn400_WhenTierNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(new TierId(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Tier?)null);

        // Act
        var resp = await _client.DeleteAsync($"/api/tiers/{id}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
            .Should().Be("Tier not found.");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Tier>(), It.IsAny<CancellationToken>()),
                         Times.Never);
    }
}

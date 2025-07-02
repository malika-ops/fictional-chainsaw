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
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.GetByIdTests;

public class GetMonetaryZoneByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IMonetaryZoneRepository> _repo = new();

    public GetMonetaryZoneByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IMonetaryZoneRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static MonetaryZone Make(Guid id, string code = "MONETARY-ZONE-001", string? name = null, bool enabled = true)
    {
        var monetaryZone = MonetaryZone.Create(
            id: MonetaryZoneId.Of(id),
            code: code,
            name: name ?? $"MonetaryZone-{code}",
            description: $"Description for {code}"
        );

        if (!enabled)
            monetaryZone.Disable();

        return monetaryZone;
    }

    private record MonetaryZoneDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/monetaryzones/{id} → 200 when MonetaryZone exists")]
    public async Task Get_ShouldReturn200_WhenMonetaryZoneExists()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "MONETARY-ZONE-123", "Euro Zone");

        _repo.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/monetaryzones/{id}");
        var body = await res.Content.ReadFromJsonAsync<MonetaryZoneDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(id);
        body.Code.Should().Be("MONETARY-ZONE-123");
        body.Name.Should().Be("Euro Zone");
        body.IsEnabled.Should().BeTrue();

        _repo.Verify(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/monetaryzones/{id} → 404 when MonetaryZone not found")]
    public async Task Get_ShouldReturn404_WhenMonetaryZoneNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((MonetaryZone?)null);

        var res = await _client.GetAsync($"/api/monetaryzones/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/monetaryzones/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/monetaryzones/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<MonetaryZoneId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/monetaryzones/{id} → 200 for disabled MonetaryZone")]
    public async Task Get_ShouldReturn200_WhenMonetaryZoneDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "MONETARY-ZONE-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(MonetaryZoneId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/monetaryzones/{id}");
        var dto = await res.Content.ReadFromJsonAsync<MonetaryZoneDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}
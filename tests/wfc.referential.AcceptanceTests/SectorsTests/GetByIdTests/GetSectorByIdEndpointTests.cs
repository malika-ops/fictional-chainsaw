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
using wfc.referential.Domain.SectorAggregate;
using wfc.referential.Domain.CityAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.SectorsTests.GetByIdTests;

public class GetSectorByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ISectorRepository> _repo = new();

    public GetSectorByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<ISectorRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Sector Make(Guid id, string code = "SECTOR-001", string? name = null, bool enabled = true)
    {
        var sector = Sector.Create(
            id: SectorId.Of(id),
            code: code,
            name: name ?? $"Sector-{code}",
            cityId: CityId.Of(Guid.NewGuid())
        );

        if (!enabled)
            sector.Disable();

        return sector;
    }

    private record SectorDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/sectors/{id} → 404 when Sector not found")]
    public async Task Get_ShouldReturn404_WhenSectorNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(SectorId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Sector?)null);

        var res = await _client.GetAsync($"/api/sectors/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(SectorId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/sectors/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/sectors/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<SectorId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/sectors/{id} → 200 for disabled Sector")]
    public async Task Get_ShouldReturn200_WhenSectorDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "SECTOR-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(SectorId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/sectors/{id}");
        var dto = await res.Content.ReadFromJsonAsync<SectorDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}
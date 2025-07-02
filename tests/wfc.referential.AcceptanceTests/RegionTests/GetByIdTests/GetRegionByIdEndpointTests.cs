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
using wfc.referential.Domain.RegionAggregate;
using wfc.referential.Domain.Countries;
using Xunit;

namespace wfc.referential.AcceptanceTests.RegionTests.GetByIdTests;

public class GetRegionByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IRegionRepository> _repo = new();

    public GetRegionByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IRegionRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static Region Make(Guid id, string code = "REGION-001", string? name = null, bool enabled = true)
    {
        var region = Region.Create(
            id: RegionId.Of(id),
            code: code,
            name: name ?? $"Region-{code}",
            countryId: CountryId.Of(Guid.NewGuid())
        );

        if (!enabled)
            region.SetInactive();

        return region;
    }

    private record RegionDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/regions/{id} → 404 when Region not found")]
    public async Task Get_ShouldReturn404_WhenRegionNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(RegionId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Region?)null);

        var res = await _client.GetAsync($"/api/regions/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(RegionId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/regions/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/regions/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<RegionId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/regions/{id} → 200 for disabled Region")]
    public async Task Get_ShouldReturn200_WhenRegionDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "REGION-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(RegionId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/regions/{id}");
        var dto = await res.Content.ReadFromJsonAsync<RegionDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}
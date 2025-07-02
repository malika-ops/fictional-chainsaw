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
using wfc.referential.Domain.ControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ControleTests.GetByIdTests;

public class GetControleByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IControleRepository> _repo = new();

    public GetControleByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IControleRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }


    private static Controle Make(Guid id, string code = "CODE-1", string? name = null, bool enabled = true)
    {
        var c = Controle.Create(ControleId.Of(id), code, name ?? $"Name-{code}");
        if (!enabled) c.Disable();
        return c;
    }

    private record ControleDto(Guid Id, string Code, string Name, bool IsEnabled);


    [Fact(DisplayName = "GET /api/controles/{id} → 200 when Controle exists")]
    public async Task Get_ShouldReturn200_WhenControleExists()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "CTL-123", "Identity-Check");

        _repo.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/controles/{id}");
        var body = await res.Content.ReadFromJsonAsync<ControleDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(id);
        body.Code.Should().Be("CTL-123");
        body.Name.Should().Be("Identity-Check");
        body.IsEnabled.Should().BeTrue();

        _repo.Verify(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/controles/{id} → 404 when Controle not found")]
    public async Task Get_ShouldReturn404_WhenControleNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Controle?)null);

        var res = await _client.GetAsync($"/api/controles/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/controles/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/controles/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<ControleId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/controles/{id} → 200 for disabled Controle")]
    public async Task Get_ShouldReturn200_WhenControleDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "CTL-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/controles/{id}");
        var dto = await res.Content.ReadFromJsonAsync<ControleDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}
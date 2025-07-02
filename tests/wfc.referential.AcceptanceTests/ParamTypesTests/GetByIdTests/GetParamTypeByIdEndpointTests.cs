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
using wfc.referential.Domain.ParamTypeAggregate;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ParamTypesTests.GetByIdTests;

public class GetParamTypeByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IParamTypeRepository> _repo = new();

    public GetParamTypeByIdEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var custom = factory.WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");

            b.ConfigureServices(s =>
            {
                s.RemoveAll<IParamTypeRepository>();
                s.RemoveAll<ICacheService>();

                s.AddSingleton(_repo.Object);
                s.AddSingleton(cacheMock.Object);
            });
        });

        _client = custom.CreateClient();
    }

    private static ParamType Make(Guid id, string code = "PARAM-TYPE-001", string? name = null, bool enabled = true)
    {
        var paramType = ParamType.Create(
            paramTypeId: ParamTypeId.Of(id),
            typeDefinitionId: TypeDefinitionId.Of(Guid.NewGuid()),
            value: name ?? $"ParamType-{code}"
        );

        if (!enabled)
            paramType.Disable();

        return paramType;
    }

    private record ParamTypeDto(Guid Id, TypeDefinitionIdDto TypeDefinitionId, string Value, bool IsEnabled);
    private record TypeDefinitionIdDto(Guid Value);

    [Fact(DisplayName = "GET /api/paramtypes/{id} → 200 when ParamType exists")]
    public async Task Get_ShouldReturn404_WhenParamTypeNotFound()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetByIdAsync(ParamTypeId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ParamType?)null);

        var res = await _client.GetAsync($"/api/paramtypes/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _repo.Verify(r => r.GetByIdAsync(ParamTypeId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/paramtypes/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/paramtypes/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<ParamTypeId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/paramtypes/{id} → 200 for disabled ParamType")]
    public async Task Get_ShouldReturn200_WhenParamTypeDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PARAM-TYPE-DIS", enabled: false);

        _repo.Setup(r => r.GetByIdAsync(ParamTypeId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/paramtypes/{id}");
        var dto = await res.Content.ReadFromJsonAsync<ParamTypeDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
}
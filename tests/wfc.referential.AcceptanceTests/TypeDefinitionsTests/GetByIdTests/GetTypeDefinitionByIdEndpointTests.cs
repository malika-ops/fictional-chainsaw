using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.TypeDefinitionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.TypeDefinitionsTests.GetByIdTests;

public class GetTypeDefinitionByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static TypeDefinition Make(Guid id, string code = "TYPE-001", string? name = null, bool enabled = true)
    {
        var typeDefinition = TypeDefinition.Create(TypeDefinitionId.Of(id), code, name ?? $"Type-{code}");
        if (!enabled) typeDefinition.Disable();
        return typeDefinition;
    }

    private record TypeDefinitionDto(Guid Id, string Code, string Name, bool IsEnabled);

    [Fact(DisplayName = "GET /api/type-definitions/{id} → 404 when TypeDefinition not found")]
    public async Task Get_ShouldReturn404_WhenTypeDefinitionNotFound()
    {
        var id = Guid.NewGuid();

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(TypeDefinitionId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((TypeDefinition?)null);

        var res = await _client.GetAsync($"/api/type-definitions/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _typeDefinitionRepoMock.Verify(r => r.GetByIdAsync(TypeDefinitionId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/type-definitions/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/type-definitions/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _typeDefinitionRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<TypeDefinitionId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/type-definitions/{id} → 200 for disabled TypeDefinition")]
    public async Task Get_ShouldReturn200_WhenTypeDefinitionDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "TYPE-DIS", enabled: false);

        _typeDefinitionRepoMock.Setup(r => r.GetByIdAsync(TypeDefinitionId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/type-definitions/{id}");
        var dto = await res.Content.ReadFromJsonAsync<TypeDefinitionDto>();

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        dto!.IsEnabled.Should().BeFalse();
    }
} 
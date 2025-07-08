using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ControleAggregate;
using Xunit;


namespace wfc.referential.AcceptanceTests.ControleTests.PatchTests;

public class PatchControleEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Controle MakeControle(Guid id, string code = "OLD", string name = "Old-Name", bool enabled = true)
    {
        var c = Controle.Create(ControleId.Of(id), code, name);
        if (!enabled) c.Disable();
        return c;
    }

    private static async Task<HttpResponseMessage> PatchAsync(HttpClient client, string url, object body)
    {
        string json = JsonSerializer.Serialize(body);
        var req = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return await client.SendAsync(req);
    }

    private static async Task<bool> ReadBoolAsync(HttpResponseMessage resp)
    {
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement;
        if (root.ValueKind is JsonValueKind.True or JsonValueKind.False) return root.GetBoolean();
        return root.GetProperty("value").GetBoolean();
    }


    [Fact(DisplayName = "PATCH /api/controles/{id} → 200 when patching Code only")]
    public async Task Patch_ShouldReturn200_WhenCodeOnly()
    {
        var id = Guid.NewGuid();
        var entity = MakeControle(id, code: "AAA");
        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        _controleRepoMock.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Controle, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync((Controle?)null);

        var payload = new { ControleId = id, Code = "NEW" };

        var resp = await PatchAsync(_client, $"/api/controles/{id}", payload);
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        entity.Code.Should().Be("NEW");
        entity.Name.Should().Be("Old-Name");
        entity.IsEnabled.Should().BeTrue();
        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/controles/{id} → 200 when toggling IsEnabled")]
    public async Task Patch_ShouldReturn200_WhenOnlyEnabled()
    {
        var id = Guid.NewGuid();
        var entity = MakeControle(id, enabled: true);
        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var payload = new { ControleId = id, IsEnabled = false };

        var resp = await PatchAsync(_client, $"/api/controles/{id}", payload);
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        entity.IsEnabled.Should().BeFalse();
        entity.Code.Should().Be("OLD");
        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/controles/{id} → 200 when patching multiple fields")]
    public async Task Patch_ShouldReturn200_WhenMultipleFields()
    {
        var id = Guid.NewGuid();
        var entity = MakeControle(id);
        _controleRepoMock.SetupSequence(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);          
        _controleRepoMock.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Controle, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync((Controle?)null);

        var payload = new { ControleId = id, Code = "XYZ", Name = "New-Name", IsEnabled = false };

        var resp = await PatchAsync(_client, $"/api/controles/{id}", payload);
        var ok = await ReadBoolAsync(resp);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        entity.Code.Should().Be("XYZ");
        entity.Name.Should().Be("New-Name");
        entity.IsEnabled.Should().BeFalse();
    }


    [Fact(DisplayName = "PATCH /api/controles/{id} → 404 when Controle not found")]
    public async Task Patch_ShouldReturn404_WhenNotFound()
    {
        var id = Guid.NewGuid();
        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Controle?)null);

        var payload = new { ControleId = id, Code = "ANY" };

        var resp = await PatchAsync(_client, $"/api/controles/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/controles/{id} → 409 when Code duplicates another Controle")]
    public async Task Patch_ShouldReturn409_WhenDuplicateCode()
    {
        var id = Guid.NewGuid();
        var duplicate = Guid.NewGuid();

        var entity = MakeControle(id, code: "AAA");
        var other = MakeControle(duplicate, code: "NEW");

        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        _controleRepoMock.Setup(r => r.GetOneByConditionAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Controle, bool>>>(),
                        It.IsAny<CancellationToken>()))
             .ReturnsAsync(other);

        var payload = new { ControleId = id, Code = "NEW" };

        var resp = await PatchAsync(_client, $"/api/controles/{id}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/controles/{id} → 400 when Code > 50 chars")]
    public async Task Patch_ShouldReturn400_WhenCodeTooLong()
    {
        var id = Guid.NewGuid();
        var entity = MakeControle(id);

        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var longCode = new string('X', 51);
        var payload = new { ControleId = id, Code = longCode };

        var resp = await PatchAsync(_client, $"/api/controles/{id}", payload);
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("Code")[0].GetString()
            .Should().Be("Code max length = 50.");

        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/controles/{id} → 400 when ControleId is empty GUID")]
    public async Task Patch_ShouldReturn400_WhenIdEmpty()
    {
        var empty = Guid.Empty;
        var payload = new { ControleId = empty, Code = "X" };

        var resp = await PatchAsync(_client, $"/api/controles/{empty}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/controles/{id} → 404 when route id malformed")]
    public async Task Patch_ShouldReturn404_WhenRouteIdMalformed()
    {
        const string bad = "not-a-guid";
        var payload = new { ControleId = Guid.NewGuid(), Code = "X" };

        var resp = await PatchAsync(_client, $"/api/controles/{bad}", payload);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
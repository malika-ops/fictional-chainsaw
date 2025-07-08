using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.ControleAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.ControleTests.DeleteTests;

public class DeleteControleEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static Controle MakeControle(Guid id, string code = "CODE")
        => Controle.Create(ControleId.Of(id), code, "Some-Name");


    [Fact(DisplayName = "DELETE /api/controles/{id} → 200 when Controle exists")]
    public async Task Delete_ShouldReturn200_WhenControleExists()
    {
        var id = Guid.NewGuid();
        var controle = MakeControle(id);

        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(controle);

        Controle? captured = null;
        _controleRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                 .Callback(() => captured = controle)
                 .Returns(Task.CompletedTask);

        var resp = await _client.DeleteAsync($"/api/controles/{id}");
        var ok = await resp.Content.ReadFromJsonAsync<bool>();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.Should().BeTrue();

        captured!.IsEnabled.Should().BeFalse();   
        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact(DisplayName = "DELETE /api/controles/{id} → 404 when Controle not found")]
    public async Task Delete_ShouldReturn404_WhenControleNotFound()
    {
        var missingId = Guid.NewGuid();

        _controleRepoMock.Setup(r => r.GetByIdAsync(ControleId.Of(missingId), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Controle?)null);

        var resp = await _client.DeleteAsync($"/api/controles/{missingId}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        doc!.RootElement.GetProperty("title").GetString().Should().Be("Resource Not Found");
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(404);

        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "DELETE /api/controles/{id} → 400 when id is empty GUID")]
    public async Task Delete_ShouldReturn400_WhenIdIsEmptyGuid()
    {
        var empty = Guid.Empty;

        var resp = await _client.DeleteAsync($"/api/controles/{empty}");
        var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>();

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("ControleId")[0].GetString()
            .Should().Be("ControleId must be a non-empty GUID.");

        _controleRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<ControleId>(), It.IsAny<CancellationToken>()), Times.Never);
        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact(DisplayName = "DELETE /api/controles/{id} → 404 when id is malformed")]
    public async Task Delete_ShouldReturn404_WhenIdMalformed()
    {
        const string badId = "not-a-guid";

        var resp = await _client.DeleteAsync($"/api/controles/{badId}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _controleRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
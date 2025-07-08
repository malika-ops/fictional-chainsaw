using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.GetByIdTests;

public class GetIdentityDocumentByIdEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static IdentityDocument Make(Guid id, string code = "CIN", string? name = null, bool enabled = true)
    {
        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(id),
            code,
            name ?? $"Document-{code}",
            "Test description");

        if (!enabled) doc.Disable();
        return doc;
    }

    [Fact(DisplayName = "GET /api/identitydocuments/{id} → 404 when IdentityDocument not found")]
    public async Task Get_ShouldReturn404_WhenIdentityDocumentNotFound()
    {
        var id = Guid.NewGuid();

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(IdentityDocumentId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync((IdentityDocument?)null);

        var res = await _client.GetAsync($"/api/identitydocuments/{id}");
        var doc = await res.Content.ReadFromJsonAsync<JsonDocument>();

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var root = doc!.RootElement;
        root.GetProperty("title").GetString().Should().Be("Resource Not Found");
        root.GetProperty("status").GetInt32().Should().Be(404);

        _identityDocumentRepoMock.Verify(r => r.GetByIdAsync(IdentityDocumentId.Of(id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "GET /api/identitydocuments/{id} → 404 when id is malformed")]
    public async Task Get_ShouldReturn404_WhenIdMalformed()
    {
        const string bad = "not-a-guid";

        var res = await _client.GetAsync($"/api/identitydocuments/{bad}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _identityDocumentRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<IdentityDocumentId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "GET /api/identitydocuments/{id} → 200 for disabled IdentityDocument")]
    public async Task Get_ShouldReturn200_WhenIdentityDocumentDisabled()
    {
        var id = Guid.NewGuid();
        var entity = Make(id, "PASSPORT", enabled: false);

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(IdentityDocumentId.Of(id), It.IsAny<CancellationToken>()))
             .ReturnsAsync(entity);

        var res = await _client.GetAsync($"/api/identitydocuments/{id}");
        var content = await res.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        root.GetProperty("isEnabled").GetString().Should().Be("False");
    }
}
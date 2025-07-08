using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.UpdateTests;

public class UpdateIdentityDocumentEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static IdentityDocument Dummy(Guid id, string code, string name) =>
        IdentityDocument.Create(IdentityDocumentId.Of(id), code, name, "desc");

    [Fact(DisplayName = "PUT /api/identitydocuments/{id} updates successfully")]
    public async Task Put_ShouldUpdateSuccessfully()
    {
        var id = Guid.NewGuid();
        var identityDocumentId = IdentityDocumentId.Of(id);
        var original = Dummy(id, "CIN", "Carte Nationale");

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(original);

        // No duplicate code exists
        _identityDocumentRepoMock.Setup(r => r.GetOneByConditionAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<IdentityDocument, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityDocument?)null);

        // Setup Update method (void method)
        _identityDocumentRepoMock.Setup(r => r.Update(It.IsAny<IdentityDocument>()));

        _identityDocumentRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var payload = new
        {
            Code = "CIN-UPD",
            Name = "Carte Modifiée",
            Description = "Nouveau texte",
            IsEnabled = false
        };

        var response = await _client.PutAsJsonAsync($"/api/identitydocuments/{id}", payload);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        original.Code.Should().Be(payload.Code);
        original.Name.Should().Be(payload.Name);
        original.Description.Should().Be(payload.Description);
        original.IsEnabled.Should().Be(payload.IsEnabled);

        _identityDocumentRepoMock.Verify(r => r.Update(original), Times.Once);
        _identityDocumentRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/identitydocuments/{id} returns 400 if name missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        var id = Guid.NewGuid();
        var payload = new
        {
            Code = "CIN-TEST",
            Description = "desc",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/identitydocuments/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        doc!.RootElement.GetProperty("errors").GetProperty("Name")[0].GetString()
            .Should().Be("Name is required.");
    }

    [Fact(DisplayName = "PUT /api/identitydocuments/{id} returns 404 if not found")]
    public async Task Put_ShouldReturn404_WhenNotFound()
    {
        var id = Guid.NewGuid(); // Use a valid GUID, not Empty
        var identityDocumentId = IdentityDocumentId.Of(id);

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((IdentityDocument?)null);

        var payload = new
        {
            Code = "XX",
            Name = "Unknown",
            Description = "x",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/identitydocuments/{id}", payload);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/identitydocuments/{id} returns 409 when code already exists")]
    public async Task Put_ShouldReturn409_WhenCodeAlreadyExists()
    {
        var id = Guid.NewGuid();
        var identityDocumentId = IdentityDocumentId.Of(id);
        var original = Dummy(id, "CIN", "Carte Nationale");

        var duplicateDoc = Dummy(Guid.NewGuid(), "DUPLICATE", "Duplicate Doc");

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(original);

        _identityDocumentRepoMock.Setup(r => r.GetOneByConditionAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<IdentityDocument, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicateDoc);

        var payload = new
        {
            Code = "DUPLICATE",
            Name = "Updated Name",
            Description = "desc",
            IsEnabled = true
        };

        var response = await _client.PutAsJsonAsync($"/api/identitydocuments/{id}", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
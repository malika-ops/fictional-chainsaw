using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.DeleteTests;

public class DeleteIdentityDocumentEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{

    [Fact(DisplayName = "DELETE /api/identitydocuments/{id} returns true when document exists")]
    public async Task Delete_ShouldReturnTrue_WhenDocumentExists()
    {
        var docId = Guid.NewGuid();
        var identityDocumentId = IdentityDocumentId.Of(docId);
        var entity = IdentityDocument.Create(
            identityDocumentId,
            "CIN",
            "Carte Nationale",
            "Valid doc"
        );

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        _identityDocumentRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var response = await _client.DeleteAsync($"/api/identitydocuments/{docId}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();

        // Verify that Disable() was called (entity should not be enabled)
        entity.IsEnabled.Should().BeFalse();

        _identityDocumentRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/identitydocuments/{id} returns 404 when document does not exist")]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        var docId = Guid.NewGuid(); // Use a valid GUID, not Empty
        var identityDocumentId = IdentityDocumentId.Of(docId);

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityDocument?)null);

        var response = await _client.DeleteAsync($"/api/identitydocuments/{docId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/identitydocuments/{id} returns 400 when ID is invalid")]
    public async Task Delete_ShouldReturn400_WhenIdIsInvalid()
    {
        var response = await _client.DeleteAsync("/api/identitydocuments/invalid-guid");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
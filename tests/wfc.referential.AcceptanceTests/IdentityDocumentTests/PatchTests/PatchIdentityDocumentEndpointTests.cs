using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.IdentityDocuments.Dtos;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.IdentityDocumentTests.PatchTests;

public class PatchIdentityDocumentEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "PATCH /api/identitydocuments/{id} returns true when updated")]
    public async Task Patch_ShouldReturnTrue_WhenValid()
    {
        var docId = Guid.NewGuid();
        var identityDocumentId = IdentityDocumentId.Of(docId);
        var existing = IdentityDocument.Create(
            identityDocumentId,
            "CIN",
            "Carte Nationale",
            "Original"
        );

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        // No duplicate code exists
        _identityDocumentRepoMock.Setup(r => r.GetOneByConditionAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<IdentityDocument, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityDocument?)null);

        // Setup Update method (void method)
        _identityDocumentRepoMock.Setup(r => r.Update(It.IsAny<IdentityDocument>()));

        // Setup SaveChangesAsync method that returns Task<int>
        _identityDocumentRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var patch = new PatchIdentityDocumentRequest
        {
            Code = "CIN-UPD",
            Name = "Carte Nationale Modifiée",
            Description = "Mise à jour",
            IsEnabled = false
        };

        var response = await _client.PatchAsync($"/api/identitydocuments/{docId}", JsonContent.Create(patch));
        var result = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();
        existing.Name.Should().Be(patch.Name);
        existing.Code.Should().Be(patch.Code);
        existing.Description.Should().Be(patch.Description);
        existing.IsEnabled.Should().Be(patch.IsEnabled!.Value);

        _identityDocumentRepoMock.Verify(r => r.Update(existing), Times.Once);
        _identityDocumentRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/identitydocuments/{id} returns 404 if not found")]
    public async Task Patch_ShouldReturn404_WhenNotFound()
    {
        var docId = Guid.NewGuid();
        var identityDocumentId = IdentityDocumentId.Of(docId);

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityDocument?)null);

        var patch = new PatchIdentityDocumentRequest
        {
            Code = "X",
            Name = "Test"
        };

        var response = await _client.PatchAsync($"/api/identitydocuments/{docId}", JsonContent.Create(patch));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PATCH /api/identitydocuments/{id} returns 400 when validation fails")]
    public async Task Patch_ShouldReturn400_WhenInvalid()
    {
        var docId = Guid.NewGuid();
        var identityDocumentId = IdentityDocumentId.Of(docId);
        var existing = IdentityDocument.Create(
            identityDocumentId,
            "CIN",
            "Carte Nationale",
            null
        );

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var patch = new PatchIdentityDocumentRequest
        {
            Code = "",  // Empty code should trigger validation error
            Name = "OK"
        };

        var response = await _client.PatchAsync($"/api/identitydocuments/{docId}", JsonContent.Create(patch));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PATCH /api/identitydocuments/{id} returns 409 when code already exists")]
    public async Task Patch_ShouldReturn409_WhenCodeAlreadyExists()
    {
        var docId = Guid.NewGuid();
        var identityDocumentId = IdentityDocumentId.Of(docId);
        var existing = IdentityDocument.Create(
            identityDocumentId,
            "CIN",
            "Carte Nationale",
            "Original"
        );

        var duplicateDoc = IdentityDocument.Create(
            IdentityDocumentId.Of(Guid.NewGuid()),
            "DUPLICATE",
            "Duplicate Doc",
            null
        );

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(identityDocumentId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        _identityDocumentRepoMock.Setup(r => r.GetOneByConditionAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<IdentityDocument, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicateDoc);

        var patch = new PatchIdentityDocumentRequest
        {
            Code = "DUPLICATE",
            Name = "Updated Name"
        };

        var response = await _client.PatchAsync($"/api/identitydocuments/{docId}", JsonContent.Create(patch));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
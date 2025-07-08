using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.DeleteTests;

public class DeleteCountryIdentityDocEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static CountryIdentityDoc CreateTestAssociation(Guid id, Guid countryId, Guid docId)
    {
        return CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(id),
            new Domain.Countries.CountryId(countryId),
            Domain.IdentityDocumentAggregate.IdentityDocumentId.Of(docId));
    }

    [Fact(DisplayName = "DELETE /api/countryidentitydocs/{id} returns 200 when association exists")]
    public async Task Delete_ShouldReturn200_WhenAssociationExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();
        var association = CreateTestAssociation(id, countryId, docId);

        _countryIdentityDocRepoMock.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(association);

        _countryIdentityDocRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/countryidentitydocs/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        association.IsEnabled.Should().BeFalse();

        _countryIdentityDocRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/countryidentitydocs/{id} returns 404 when association not found")]
    public async Task Delete_ShouldReturn404_WhenAssociationNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _countryIdentityDocRepoMock.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((CountryIdentityDoc?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/countryidentitydocs/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryIdentityDocRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
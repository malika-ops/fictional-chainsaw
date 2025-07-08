using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.PatchTests;

public class PatchCountryIdentityDocEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    private static CountryIdentityDoc CreateTestAssociation(Guid id, Guid countryId, Guid docId)
    {
        return CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(id),
            new CountryId(countryId),
            IdentityDocumentId.Of(docId));
    }

    [Fact(DisplayName = "PATCH /api/countryidentitydocs/{id} returns 200 and patches only the provided fields")]
    public async Task Patch_ShouldReturn200_AndPatchOnlyProvidedFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldCountryId = Guid.NewGuid();
        var oldDocId = Guid.NewGuid();
        var newCountryId = Guid.NewGuid();

        var association = CreateTestAssociation(id, oldCountryId, oldDocId);

        _countryIdentityDocRepoMock.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(association);

        var country = Country.Create(
            new CountryId(newCountryId),
            "FR",
            "France",
            "FRA",
            "FR",
            "FRA",
            "+33",
            "GMT+1",
            false,
            false,
            2,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid()),
            new Domain.CurrencyAggregate.CurrencyId(Guid.NewGuid())
        );

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(newCountryId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        _countryIdentityDocRepoMock.Setup(r => r.Update(It.IsAny<CountryIdentityDoc>()));

        _countryIdentityDocRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var payload = new PatchCountryIdentityDocRequest
        {
            CountryId = newCountryId,
            // IdentityDocumentId intentionally omitted - should not change
            // IsEnabled also omitted
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/countryidentitydocs/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        association.CountryId.Value.Should().Be(newCountryId);
        association.IdentityDocumentId.Value.Should().Be(oldDocId);  // Should not change
        association.IsEnabled.Should().BeTrue();  // Should not change

        _countryIdentityDocRepoMock.Verify(r => r.Update(It.IsAny<CountryIdentityDoc>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/countryidentitydocs/{id} returns 404 when association doesn't exist")]
    public async Task Patch_ShouldReturn404_WhenAssociationDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _countryIdentityDocRepoMock.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((CountryIdentityDoc?)null);

        var payload = new PatchCountryIdentityDocRequest
        {
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/countryidentitydocs/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryIdentityDocRepoMock.Verify(r => r.Update(It.IsAny<CountryIdentityDoc>()), Times.Never);
    }
}
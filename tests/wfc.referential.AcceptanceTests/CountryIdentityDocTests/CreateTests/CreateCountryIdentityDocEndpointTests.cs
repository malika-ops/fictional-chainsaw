using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.CreateTests;

public class CreateCountryIdentityDocEndpointTests(TestWebApplicationFactory factory) : BaseAcceptanceTests(factory)
{
    [Fact(DisplayName = "POST /api/countryidentitydocs returns 200 and Guid when valid")]
    public async Task Post_ShouldReturn200_WhenRequestIsValid()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        var country = Country.Create(
            new CountryId(countryId),
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
            new MonetaryZoneId(Guid.NewGuid()),
            new CurrencyId(Guid.NewGuid())
        );

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte d'identité",
            "Description"
            );

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(countryId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(IdentityDocumentId.Of(docId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        _countryIdentityDocRepoMock.Setup(r => r.AddAsync(It.IsAny<CountryIdentityDoc>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CountryIdentityDoc c, CancellationToken _) => c);

        _countryIdentityDocRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var payload = new
        {
            CountryId = countryId,
            IdentityDocumentId = docId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countryidentitydocs", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _countryIdentityDocRepoMock.Verify(r => r.AddAsync(
                            It.Is<CountryIdentityDoc>(c =>
                                c.CountryId.Value == countryId &&
                                c.IdentityDocumentId.Value == docId &&
                                c.IsEnabled == true),
                            It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "POST /api/countryidentitydocs returns 404 when Country not found")]
    public async Task Post_ShouldReturn404_WhenCountryNotFound()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(countryId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Country?)null);

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte d'identité",
            "Description"
        );

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(IdentityDocumentId.Of(docId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        var payload = new
        {
            CountryId = countryId,
            IdentityDocumentId = docId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countryidentitydocs", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _countryIdentityDocRepoMock.Verify(r => r.AddAsync(
                            It.IsAny<CountryIdentityDoc>(),
                            It.IsAny<CancellationToken>()),
                        Times.Never);
    }

    [Fact(DisplayName = "POST /api/countryidentitydocs returns 409 when association exists")]
    public async Task Post_ShouldReturn409_WhenAssociationAlreadyExists()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        var country = Country.Create(
            new CountryId(countryId),
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
            new MonetaryZoneId(Guid.NewGuid()),
            new CurrencyId(Guid.NewGuid())
        );

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte d'identité",
            "Description"
            );

        // Create an existing association to simulate the conflict
        var existingAssociation = CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(Guid.NewGuid()),
            CountryId.Of(countryId),
            IdentityDocumentId.Of(docId)
        );

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(countryId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        _identityDocumentRepoMock.Setup(r => r.GetByIdAsync(IdentityDocumentId.Of(docId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        // Mock the GetByConditionAsync to return the existing association
        _countryIdentityDocRepoMock.Setup(r => r.GetByConditionAsync(
                            It.IsAny<Expression<Func<CountryIdentityDoc, bool>>>(),
                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryIdentityDoc> { existingAssociation });

        var payload = new
        {
            CountryId = countryId,
            IdentityDocumentId = docId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countryidentitydocs", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        _countryIdentityDocRepoMock.Verify(r => r.AddAsync(
                            It.IsAny<CountryIdentityDoc>(),
                            It.IsAny<CancellationToken>()),
                        Times.Never);
    }
}
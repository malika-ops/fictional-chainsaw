using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.UpdateTests;

public class UpdateCountryIdentityDocEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryIdentityDocRepository> _repoMock = new();
    private readonly Mock<ICountryRepository> _countryRepoMock = new();
    private readonly Mock<IIdentityDocumentRepository> _identityDocRepoMock = new();

    public UpdateCountryIdentityDocEndpointTests(WebApplicationFactory<Program> factory)
    {
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICountryIdentityDocRepository>();
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<IIdentityDocumentRepository>();

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(_identityDocRepoMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static CountryIdentityDoc CreateTestCountryIdentityDoc(Guid id, Guid countryId, Guid docId)
    {
        return CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(id),
            new CountryId(countryId),
            IdentityDocumentId.Of(docId));
    }

    [Fact(DisplayName = "PUT /api/countryidentitydocs/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldCountryId = Guid.NewGuid();
        var oldDocId = Guid.NewGuid();
        var newCountryId = Guid.NewGuid();
        var newDocId = Guid.NewGuid();

        var old = CreateTestCountryIdentityDoc(id, oldCountryId, oldDocId);

        _repoMock.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(old);

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
            new CurrencyId(Guid.NewGuid())
        );

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(newDocId),
            "CIN",
            "Carte d'identité",
            "Description"
        );

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(newCountryId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        _identityDocRepoMock.Setup(r => r.GetByIdAsync(IdentityDocumentId.Of(newDocId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        _repoMock.Setup(r => r.Update(It.IsAny<CountryIdentityDoc>()));

        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var payload = new
        {
            CountryIdentityDocId = id,
            CountryId = newCountryId,
            IdentityDocumentId = newDocId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/countryidentitydocs/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().BeTrue();

        old.CountryId.Value.Should().Be(newCountryId);
        old.IdentityDocumentId.Value.Should().Be(newDocId);
        old.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.Update(old), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/countryidentitydocs/{id} returns 404 when country not found")]
    public async Task Put_ShouldReturn404_WhenCountryNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldCountryId = Guid.NewGuid();
        var oldDocId = Guid.NewGuid();
        var newCountryId = Guid.NewGuid();
        var newDocId = Guid.NewGuid();

        var old = CreateTestCountryIdentityDoc(id, oldCountryId, oldDocId);

        _repoMock.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(old);

        _countryRepoMock.Setup(r => r.GetByIdAsync(CountryId.Of(newCountryId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Country?)null);

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(newDocId),
            "CIN",
            "Carte d'identité",
            "Description"
        );

        _identityDocRepoMock.Setup(r => r.GetByIdAsync(IdentityDocumentId.Of(newDocId), It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        var payload = new
        {
            CountryIdentityDocId = id,
            CountryId = newCountryId,
            IdentityDocumentId = newDocId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/countryidentitydocs/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.Update(It.IsAny<CountryIdentityDoc>()), Times.Never);
    }
}
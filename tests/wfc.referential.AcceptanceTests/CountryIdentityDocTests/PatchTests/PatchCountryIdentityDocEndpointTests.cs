using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.CountryIdentityDocs.Dtos;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CountryIdentityDocAggregate;
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.PatchTests;

public class PatchCountryIdentityDocEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryIdentityDocRepository> _repoMock = new();
    private readonly Mock<ICountryRepository> _countryRepoMock = new();
    private readonly Mock<IIdentityDocumentRepository> _identityDocRepoMock = new();

    public PatchCountryIdentityDocEndpointTests(WebApplicationFactory<Program> factory)
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

        _repoMock.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
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
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid()),
            new Domain.CurrencyAggregate.CurrencyId(Guid.NewGuid())
        );

        _countryRepoMock.Setup(r => r.GetByIdAsync(newCountryId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        _repoMock.Setup(r => r.Update(It.IsAny<CountryIdentityDoc>()));

        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var payload = new PatchCountryIdentityDocRequest
        {
            CountryIdentityDocId = id,
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

        _repoMock.Verify(r => r.Update(It.IsAny<CountryIdentityDoc>()), Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/countryidentitydocs/{id} returns 404 when association doesn't exist")]
    public async Task Patch_ShouldReturn404_WhenAssociationDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(CountryIdentityDocId.Of(id), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((CountryIdentityDoc?)null);

        var payload = new PatchCountryIdentityDocRequest
        {
            CountryIdentityDocId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/countryidentitydocs/{id}", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r => r.Update(It.IsAny<CountryIdentityDoc>()), Times.Never);
    }
}
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Application.Interfaces;
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
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICountryIdentityDocRepository>();
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<IIdentityDocumentRepository>();
                services.RemoveAll<ICacheService>();

                // Comportement par défaut pour Update
                _repoMock
                    .Setup(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(_identityDocRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static CountryIdentityDoc CreateTestCountryIdentityDoc(Guid id, Guid countryId, Guid docId)
    {
        return CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(id),
            new CountryId(countryId),
            IdentityDocumentId.Of(docId),
            true
        );
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

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
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
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid()),
            new CurrencyId(Guid.NewGuid())
        );

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(newDocId),
            "CIN",
            "Carte d'identité",
            "Description"
        );

        _countryRepoMock.Setup(r => r.GetByIdAsync(newCountryId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        _identityDocRepoMock.Setup(r => r.GetByIdAsync(newDocId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        CountryIdentityDoc? updated = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(), It.IsAny<CancellationToken>()))
                 .Callback<CountryIdentityDoc, CancellationToken>((c, _) => updated = c)
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
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.CountryId.Value.Should().Be(newCountryId);
        updated.IdentityDocumentId.Value.Should().Be(newDocId);
        updated.IsEnabled.Should().BeTrue();

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(),
                                           It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "PUT /api/countryidentitydocs/{id} returns 400 when country not found")]
    public async Task Put_ShouldReturn400_WhenCountryNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oldCountryId = Guid.NewGuid();
        var oldDocId = Guid.NewGuid();
        var newCountryId = Guid.NewGuid();
        var newDocId = Guid.NewGuid();

        var old = CreateTestCountryIdentityDoc(id, oldCountryId, oldDocId);

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(old);

        _countryRepoMock.Setup(r => r.GetByIdAsync(newCountryId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Country?)null);

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(newDocId),
            "CIN",
            "Carte d'identité",
            "Description"
        );

        _identityDocRepoMock.Setup(r => r.GetByIdAsync(newDocId, It.IsAny<CancellationToken>()))
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
        var jsonResult = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        jsonResult!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Country with ID {newCountryId} not found");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(),
                                           It.IsAny<CancellationToken>()),
                        Times.Never);
    }
}
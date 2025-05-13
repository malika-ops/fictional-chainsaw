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
using wfc.referential.Domain.IdentityDocumentAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.CreateTests;

public class CreateCountryIdentityDocEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryIdentityDocRepository> _repoMock = new();
    private readonly Mock<ICountryRepository> _countryRepoMock = new();
    private readonly Mock<IIdentityDocumentRepository> _identityDocRepoMock = new();

    public CreateCountryIdentityDocEndpointTests(WebApplicationFactory<Program> factory)
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

                // Comportement par défaut pour AddAsync
                _repoMock
                    .Setup(r => r.AddAsync(It.IsAny<CountryIdentityDoc>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((CountryIdentityDoc c, CancellationToken _) => c);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(_identityDocRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

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
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid())
        );

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte d'identité",
            "Description",
            true
        );

        _countryRepoMock.Setup(r => r.GetByIdAsync(countryId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        _identityDocRepoMock.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        _repoMock.Setup(r => r.ExistsByCountryAndIdentityDocumentAsync(
                        It.IsAny<CountryId>(),
                        It.IsAny<IdentityDocumentId>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

        var payload = new
        {
            CountryId = countryId,
            IdentityDocumentId = docId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countryidentitydocs", payload);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedId.Should().NotBeEmpty();

        _repoMock.Verify(r => r.AddAsync(
                            It.Is<CountryIdentityDoc>(c =>
                                c.CountryId.Value == countryId &&
                                c.IdentityDocumentId.Value == docId &&
                                c.IsEnabled == true),
                            It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "POST /api/countryidentitydocs returns 400 when Country not found")]
    public async Task Post_ShouldReturn400_WhenCountryNotFound()
    {
        // Arrange
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        _countryRepoMock.Setup(r => r.GetByIdAsync(countryId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Country?)null);

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte d'identité",
            "Description",
            true
        );

        _identityDocRepoMock.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        var payload = new
        {
            CountryId = countryId,
            IdentityDocumentId = docId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countryidentitydocs", payload);
        var jsonResult = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        jsonResult!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Country with ID {countryId} not found");

        _repoMock.Verify(r => r.AddAsync(
                            It.IsAny<CountryIdentityDoc>(),
                            It.IsAny<CancellationToken>()),
                        Times.Never);
    }

    [Fact(DisplayName = "POST /api/countryidentitydocs returns 400 when association exists")]
    public async Task Post_ShouldReturn400_WhenAssociationAlreadyExists()
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
            true,
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid())
        );

        var doc = IdentityDocument.Create(
            IdentityDocumentId.Of(docId),
            "CIN",
            "Carte d'identité",
            "Description",
            true
        );

        _countryRepoMock.Setup(r => r.GetByIdAsync(countryId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        _identityDocRepoMock.Setup(r => r.GetByIdAsync(docId, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(doc);

        _repoMock.Setup(r => r.ExistsByCountryAndIdentityDocumentAsync(
                        It.Is<CountryId>(c => c.Value == countryId),
                        It.Is<IdentityDocumentId>(d => d.Value == docId),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

        var payload = new
        {
            CountryId = countryId,
            IdentityDocumentId = docId,
            IsEnabled = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/countryidentitydocs", payload);
        var jsonResult = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        jsonResult!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"Country identity document association with CountryId: {countryId} and IdentityDocumentId: {docId} already exists.");

        _repoMock.Verify(r => r.AddAsync(
                            It.IsAny<CountryIdentityDoc>(),
                            It.IsAny<CancellationToken>()),
                        Times.Never);
    }
}
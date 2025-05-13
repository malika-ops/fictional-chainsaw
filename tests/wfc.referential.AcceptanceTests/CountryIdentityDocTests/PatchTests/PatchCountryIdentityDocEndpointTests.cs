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

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_countryRepoMock.Object);
                services.AddSingleton(_identityDocRepoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static CountryIdentityDoc CreateTestAssociation(Guid id, Guid countryId, Guid docId)
    {
        return CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(id),
            new CountryId(countryId),
            IdentityDocumentId.Of(docId),
            true
        );
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

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
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
            new Domain.MonetaryZoneAggregate.MonetaryZoneId(Guid.NewGuid())
        );

        _countryRepoMock.Setup(r => r.GetByIdAsync(newCountryId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(country);

        CountryIdentityDoc? updated = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(), It.IsAny<CancellationToken>()))
                 .Callback<CountryIdentityDoc, CancellationToken>((c, _) => updated = c)
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
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.CountryId.Value.Should().Be(newCountryId);
        updated.IdentityDocumentId.Value.Should().Be(oldDocId);  // Should not change
        updated.IsEnabled.Should().BeTrue();  // Should not change

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(),
                                           It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/countryidentitydocs/{id} returns 200 and updates IsEnabled")]
    public async Task Patch_ShouldReturn200_AndUpdateIsEnabled()
    {
        // Arrange
        var id = Guid.NewGuid();
        var countryId = Guid.NewGuid();
        var docId = Guid.NewGuid();

        var association = CreateTestAssociation(id, countryId, docId);

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(association);

        CountryIdentityDoc? updated = null;
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(), It.IsAny<CancellationToken>()))
                 .Callback<CountryIdentityDoc, CancellationToken>((c, _) => updated = c)
                 .Returns(Task.CompletedTask);

        var payload = new PatchCountryIdentityDocRequest
        {
            CountryIdentityDocId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/countryidentitydocs/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(id);

        updated!.IsEnabled.Should().BeFalse();
        updated.CountryId.Value.Should().Be(countryId);  // Should not change
        updated.IdentityDocumentId.Value.Should().Be(docId);  // Should not change

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(),
                                           It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "PATCH /api/countryidentitydocs/{id} returns 400 when association doesn't exist")]
    public async Task Patch_ShouldReturn400_WhenAssociationDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((CountryIdentityDoc?)null);

        var payload = new PatchCountryIdentityDocRequest
        {
            CountryIdentityDocId = id,
            IsEnabled = false
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/countryidentitydocs/{id}", payload);
        var jsonResult = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        jsonResult!.RootElement.GetProperty("errors").GetString()
           .Should().Be("CountryIdentityDoc not found");

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(),
                                           It.IsAny<CancellationToken>()),
                        Times.Never);
    }
}
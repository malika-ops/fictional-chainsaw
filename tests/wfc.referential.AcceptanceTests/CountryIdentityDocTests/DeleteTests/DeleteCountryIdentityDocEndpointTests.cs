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
using wfc.referential.Domain.CountryIdentityDocAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CountryIdentityDocTests.DeleteTests;

public class DeleteCountryIdentityDocEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICountryIdentityDocRepository> _repoMock = new();

    public DeleteCountryIdentityDocEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ICountryIdentityDocRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static CountryIdentityDoc CreateTestAssociation(Guid id, Guid countryId, Guid docId)
    {
        return CountryIdentityDoc.Create(
            CountryIdentityDocId.Of(id),
            new Domain.Countries.CountryId(countryId),
            Domain.IdentityDocumentAggregate.IdentityDocumentId.Of(docId),
            true
        );
    }

    [Fact(DisplayName = "DELETE /api/countryidentitydocs/{id} returns 200 when association exists")]
    public async Task Delete_ShouldReturn200_WhenAssociationExists()
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

        // Act
        var response = await _client.DeleteAsync($"/api/countryidentitydocs/{id}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updated!.IsEnabled.Should().BeFalse();

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<CountryIdentityDoc>(),
                                           It.IsAny<CancellationToken>()),
                        Times.Once);
    }

    [Fact(DisplayName = "DELETE /api/countryidentitydocs/{id} returns 400 when association not found")]
    public async Task Delete_ShouldReturn400_WhenAssociationNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((CountryIdentityDoc?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/countryidentitydocs/{id}");
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
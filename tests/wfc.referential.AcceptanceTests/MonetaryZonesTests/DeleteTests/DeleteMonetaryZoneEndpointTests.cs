using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.MonetaryZoneAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.MonetaryZonesTests.DeleteTests;

public class DeleteMonetaryZoneEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IMonetaryZoneRepository> _repoMock = new();

    public DeleteMonetaryZoneEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMonetaryZoneRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                          It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    // 1) Happy‑path: disable succeeds
    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} returns 200 when zone exists and has no countries")]
    public async Task Delete_ShouldReturn200_WhenZoneExistsAndHasNoCountries()
    {
        // Arrange
        var id = MonetaryZoneId.Of(Guid.NewGuid());
        var code = "SEK";

        var zone = MonetaryZone.Create(id, code, "Swedish Krona", "Sweden",
                                       new List<Country>());

        _repoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(zone);

        // capture the entity passed to Update
        MonetaryZone? updatedZone = null;
        _repoMock
            .Setup(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(), It.IsAny<CancellationToken>()))
            .Callback<MonetaryZone, CancellationToken>((mz, _) => updatedZone = mz)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/api/monetaryZones/{id.Value}");
        var body = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().BeTrue();

        updatedZone!.IsEnabled.Should().Be(false);

        _repoMock.Verify(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                        It.IsAny<CancellationToken>()),
                                                        Times.Once);
    }

    // 2) Zone not found
    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} returns 400 when zone is not found")]
    public async Task Delete_ShouldReturn400_WhenZoneNotFound()
    {
        // Arrange
        var id = MonetaryZoneId.Of(Guid.NewGuid());

        _repoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonetaryZone?)null);

        // Act
        var response = await _client.DeleteAsync($"/api/monetaryZones/{id.Value}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Monetary zone not found");

        _repoMock.Verify(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                        It.IsAny<CancellationToken>()),
                                                        Times.Never);
    }

    // 3) Zone has linked countries
    [Fact(DisplayName = "DELETE /api/monetaryZones/{id} returns 400 when zone has countries")]
    public async Task Delete_ShouldReturn400_WhenZoneHasCountries()
    {
        // Arrange
        var id = MonetaryZoneId.Of(Guid.NewGuid());
        var code = "CAD";

        // list with one dummy entry → count > 0
        var zoneWithCountries = MonetaryZone.Create(id, code, "Canadian Dollar", "Canada",
                                                    new List<Country> { null! });

        _repoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(zoneWithCountries);

        // Act
        var response = await _client.DeleteAsync($"/api/monetaryZones/{id.Value}");
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be("Can not delete a Monetary Zone with existing Countries");

        _repoMock.Verify(r => r.UpdateMonetaryZoneAsync(It.IsAny<MonetaryZone>(),
                                                        It.IsAny<CancellationToken>()),
                                                        Times.Never);
    }
}
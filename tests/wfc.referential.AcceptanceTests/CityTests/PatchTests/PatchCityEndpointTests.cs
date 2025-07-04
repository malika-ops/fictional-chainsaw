using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Cities.Dtos;
using wfc.referential.Application.Constants;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.CityAggregate;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.CityTests.PatchTests;

public class PatchCityEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<ICityRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    public PatchCityEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<ICityRepository>();
                services.RemoveAll<ICacheService>();

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "PATCH /api/cities/{id} updates the city successfully")]
    public async Task PatchCity_ShouldReturnUpdatedCityId_WhenCityExists()
    {
        // Arrange
        var cityId = CityId.Of(Guid.NewGuid());
        var patchRequest = new PatchCityRequest
        {
            Code = "new-code",
            Name = "Updated Name",
        };

        var city = City.Create(cityId, "code", "name", "timezone", RegionId.Of(Guid.NewGuid()), "abbrev");

        _repoMock.Setup(r => r.GetOneByConditionAsync(c => c.Id == cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        // Act
        var response = await _client.PatchAsync($"/api/cities/{cityId}", JsonContent.Create(patchRequest));
        var updatedRegionId = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedRegionId.Should().Be(true);
        city.Name.Should().BeEquivalentTo(patchRequest.Name);

        _repoMock.Verify(r =>
            r.Update(It.Is<City>(c => c.Name == patchRequest.Name && c.Code == patchRequest.Code)), Times.Once);

    }


    [Fact(DisplayName = "PATCH /api/cities/{id} returns 404 when city does not exist")]
    public async Task PatchCity_ShouldReturnNotFound_WhenCityDoesNotExist()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var patchRequest = new PatchCityRequest
        {
            Code = "non-existing-code",
            Name = "Non-existing Region",
        };

        _repoMock.Setup(r => r.GetOneByConditionAsync(c => c.Id == CityId.Of(cityId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((City)null);

        // Act
        var response = await _client.PatchAsync($"/api/cities/{cityId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _repoMock.Verify(r =>
            r.Update(It.IsAny<City>()),
            Times.Never);

        _cacheMock.Verify(c =>
            c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "PATCH /api/cities/{id} returns 400 when validation fails")]
    public async Task PatchCity_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var patchRequest = new PatchCityRequest
        {
            Code = "", // Champ vide invalide
            Name = "Invalid City"
        };
        // Act
        var response = await _client.PatchAsync($"/api/cities/{cityId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        _repoMock.Verify(r =>
            r.Update(It.IsAny<City>()),
            Times.Never);

        _cacheMock.Verify(c =>
            c.RemoveByPrefixAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

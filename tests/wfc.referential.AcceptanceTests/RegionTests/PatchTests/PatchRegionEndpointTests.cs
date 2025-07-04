using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using wfc.referential.Application.Interfaces;
using wfc.referential.Application.Regions.Dtos;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.CurrencyAggregate;
using wfc.referential.Domain.MonetaryZoneAggregate;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.RegionTests.PatchTests;

public class PatchRegionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IRegionRepository> _repoMock = new();
    private readonly Mock<ICountryRepository> _repoCountryMock = new();

    public PatchRegionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        // Clone the factory and customize the host
        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 🧹 Remove concrete registrations that hit the DB / Redis
                services.RemoveAll<IRegionRepository>();
                services.RemoveAll<ICountryRepository>();
                services.RemoveAll<ICacheService>();

                // 🔌 Plug mocks back in
                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(_repoCountryMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    [Fact(DisplayName = "PATCH /api/regions/{id} updates the region successfully")]
    public async Task PatchRegion_ShouldReturnUpdatedRegionId_WhenRegionExists()
    {
        // Arrange
        var regionId = Guid.NewGuid();
        var patchRequest = new PatchRegionRequest
        {
            Code = "new-code",
            Name = "Updated Name",
            IsEnabled = true,
            CountryId = Guid.NewGuid()
        };

        var region = Region.Create(
            RegionId.Of(regionId),
            "old-code",
            "Old Name",
            CountryId.Of(Guid.NewGuid()) // Assuming this is a valid country ID
        );
        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Region, bool>> predicate, CancellationToken _) =>
            {
                var func = predicate.Compile();

                if (func(region))
                    return region;

                return null;
            });
        _repoCountryMock.Setup(
            r => r.GetByIdAsync(It.IsAny<CountryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Country.Create(
                CountryId.Of(patchRequest.CountryId.Value),
                "Morocco",
                "MA",
                "Moroccan Dirham", "iso2", "iso3", "dialingCode", "GMT", true, true, 2,
                MonetaryZoneId.Of(Guid.NewGuid()), CurrencyId.Of(Guid.NewGuid())));
        // Act
        var response = await _client.PatchAsync($"/api/regions/{regionId}", JsonContent.Create(patchRequest));
        var updatedRegionId = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedRegionId.Should().Be(true);
        region.Name.Should().BeEquivalentTo(patchRequest.Name);
    }


    [Fact(DisplayName = "PATCH /api/regions/{id} returns 404 when region does not exist")]
        public async Task PatchRegion_ShouldReturnNotFound_WhenRegionDoesNotExist()
    {
        // Arrange
        var regionId = Guid.NewGuid();
        var patchRequest = new PatchRegionRequest
        {
            Code = "non-existing-code",
            Name = "Non-existing Region",
        };

        _repoMock.Setup(r => r.GetOneByConditionAsync(r => r.Id == RegionId.Of(regionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Region)null); 

        // Act
        var response = await _client.PatchAsync($"/api/regions/{regionId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PATCH /api/regions/{id} returns 400 when validation fails")]
    public async Task PatchRegion_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var regionId = Guid.NewGuid();
        var patchRequest = new PatchRegionRequest
        {
            Code = "", // Assuming empty code is invalid
            Name = "Invalid Region",
        };

        _repoMock.Setup(r => r.GetOneByConditionAsync(r => r.Id == RegionId.Of(regionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Region.Create(RegionId.Of(regionId), "code", "name", CountryId.Of(Guid.NewGuid())));

        // Act
        var response = await _client.PatchAsync($"/api/regions/{regionId}", JsonContent.Create(patchRequest));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

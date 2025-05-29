using BuildingBlocks.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using wfc.referential.Application.Interfaces;
using wfc.referential.Domain.Countries;
using wfc.referential.Domain.RegionAggregate;
using Xunit;

namespace wfc.referential.AcceptanceTests.RegionTests.UpdateTests;

public class UpdateRegionEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IRegionRepository> _repoMock = new();

    public UpdateRegionEndpointTests(WebApplicationFactory<Program> factory)
    {
        var cacheMock = new Mock<ICacheService>();

        var customisedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRegionRepository>();
                services.RemoveAll<ICacheService>();

                _repoMock
                    .Setup(r => r.Update(It.IsAny<Region>()));

                services.AddSingleton(_repoMock.Object);
                services.AddSingleton(cacheMock.Object);
            });
        });

        _client = customisedFactory.CreateClient();
    }

    private static Region DummyRegion(Guid id, string code, string name) =>
        Region.Create(RegionId.Of(id), code, name, CountryId.Of(Guid.NewGuid()));

    [Fact(DisplayName = "PUT /api/regions/{id} returns 200 when update succeeds")]
    public async Task Put_ShouldReturn200_WhenUpdateIsSuccessful()
    {
        var id = Guid.NewGuid();
        var oldRegion = DummyRegion(id, "NA", "North America");

        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Region, bool>> predicate, CancellationToken _) =>
            {
                var func = predicate.Compile();

                if (func(oldRegion))
                    return oldRegion;

                return null;
            });

        Region? updated = null;
        _repoMock.Setup(r => r.Update(oldRegion))
                 .Callback<Region>((rg) => updated = rg);

        var payload = new
        {
            Code = "SA",
            Name = "South America",
            IsEnabled= true,
            CountryId = CountryId.Of(Guid.NewGuid())
        };

        var response = await _client.PutAsJsonAsync($"/api/regions/{id}", payload);
        var returned = await response.Content.ReadFromJsonAsync<bool>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().Be(true);

        updated!.Code.Should().Be("SA");
        updated.Name.Should().Be("South America");

        _repoMock.Verify(r => r.Update(It.IsAny<Region>()), Times.Once);
    }

    [Fact(DisplayName = "PUT /api/regions/{id} returns 400 when Name is missing")]
    public async Task Put_ShouldReturn400_WhenNameMissing()
    {
        var id = Guid.NewGuid();
        var payload = new
        {
            Code = "AF",
            CountryId = CountryId.Of(Guid.NewGuid())
        };

        var response = await _client.PutAsJsonAsync($"/api/regions/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        doc!.RootElement.GetProperty("errors")
            .GetProperty("name")[0].GetString()
            .Should().Be("Name is required");

        _repoMock.Verify(r => r.Update(It.IsAny<Region>()), Times.Never);
    }

    [Fact(DisplayName = "PUT /api/regions/{id} returns 409 Conflict when new code already exists")]
    public async Task Put_ShouldReturn409Conflict_WhenCodeAlreadyExists()
    {
        var id = Guid.NewGuid();
        var existing = DummyRegion(Guid.NewGuid(), "EU", "Europe");
        var target = DummyRegion(id, "AS", "Asia");


        _repoMock.Setup(r => r.GetOneByConditionAsync(It.IsAny<Expression<Func<Region, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Region, bool>> predicate, CancellationToken _) =>
            {
                var func = predicate.Compile();

                if (func(target))
                    return target;

                return existing;
            });

        var payload = new
        {
            RegionId = id,
            Code = "EU", // duplicate code
            Name = "Europe",
            IsEnabled = true,
            CountryId = CountryId.Of(Guid.NewGuid())
        };

        var response = await _client.PutAsJsonAsync($"/api/regions/{id}", payload);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        doc!.RootElement.GetProperty("errors").GetString()
           .Should().Be($"{nameof(Region)} with code : {payload.Code} already exist");

        _repoMock.Verify(r => r.Update(It.IsAny<Region>()), Times.Never);
    }
}
